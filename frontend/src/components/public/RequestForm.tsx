import { useEffect, useMemo, useRef, useState } from 'react'
import heic2any from 'heic2any'
import { assets } from '../../services/mockData'
import type { CreateDamageRequestPayload } from '../../types/models'
import { Button } from '../shared/Button'
import { Input } from '../shared/Input'
import { Modal } from '../shared/Modal'
import { isValidRuPhone, maskRuPhone } from '../../services/format'
import { useI18n } from '../../services/i18n'

type RequestFormProps = {
  open: boolean
  submitting: boolean
  onClose: () => void
  onSubmit: (payload: CreateDamageRequestPayload) => Promise<void>
  onSuccess: () => void
  onError: (message: string) => void
}

type RequestFormData = {
  firstName: string
  lastName: string
  middleName: string
  email: string
  phone: string
  carBrand: string
  carModel: string
  carYear: string
  agree: boolean
  files: File[]
}

type RequestFormErrors = Partial<Record<keyof RequestFormData, string>>

const initialForm: RequestFormData = {
  firstName: '',
  lastName: '',
  middleName: '',
  email: '',
  phone: '',
  carBrand: '',
  carModel: '',
  carYear: '',
  agree: false,
  files: [],
}

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const submitProgressAnimationMs = 900
const maxUploadFiles = 3
const allowedFileExtensions = new Set(['jpg', 'jpeg', 'png', 'webp', 'heic', 'heif'])
const allowedMimeTypes = new Set([
  'image/jpeg',
  'image/png',
  'image/webp',
  'image/heic',
  'image/heif',
  'image/jpg',
])

const getFileExtension = (fileName: string) => {
  const parts = fileName.toLowerCase().split('.')
  return parts.length > 1 ? parts[parts.length - 1] : ''
}

const isHeicLikeFile = (file: File) => {
  const extension = getFileExtension(file.name)
  const mimeType = file.type.toLowerCase()
  return extension === 'heic' || extension === 'heif' || mimeType.includes('heic') || mimeType.includes('heif')
}

const toJpegFileName = (fileName: string) => {
  const lastDotIndex = fileName.lastIndexOf('.')
  const baseName = lastDotIndex > 0 ? fileName.slice(0, lastDotIndex) : fileName
  return `${baseName}.jpg`
}

const isAllowedImageFile = (file: File) => {
  const extension = getFileExtension(file.name)
  const mimeType = file.type.toLowerCase()
  return allowedFileExtensions.has(extension) || allowedMimeTypes.has(mimeType)
}

const convertHeicToJpeg = async (file: File) => {
  if (!isHeicLikeFile(file)) {
    return file
  }

  const decodeFallbackFile = await convertHeicToJpegWithDecodeFallback(file)
  if (decodeFallbackFile) {
    return decodeFallbackFile
  }

  const sourceBlob = new Blob([await file.arrayBuffer()], {
    type: file.type || 'image/heic',
  })

  const attempts: Array<{ blob: Blob; quality: number }> = [
    { blob: sourceBlob, quality: 0.9 },
    { blob: sourceBlob, quality: 0.8 },
    { blob: file, quality: 0.9 },
    { blob: file, quality: 0.8 },
  ]

  for (const attempt of attempts) {
    try {
      const conversionResult = await heic2any({
        blob: attempt.blob,
        toType: 'image/jpeg',
        quality: attempt.quality,
      })
      const finalBlob = Array.isArray(conversionResult) ? conversionResult[0] : conversionResult

      if (finalBlob instanceof Blob) {
        return new File([finalBlob], toJpegFileName(file.name), {
          type: 'image/jpeg',
          lastModified: file.lastModified,
        })
      }
    } catch {
    }
  }

  throw new Error('HEIC conversion failed')
}

const convertHeicToJpegWithDecodeFallback = async (file: File): Promise<File | null> => {
  try {
    const { default: decodeHeic } = await import('heic-decode')
    const decoded = await decodeHeic({ buffer: new Uint8Array(await file.arrayBuffer()) })
    if (!decoded || decoded.width < 1 || decoded.height < 1 || decoded.data.length === 0) {
      return null
    }

    const canvas = document.createElement('canvas')
    canvas.width = decoded.width
    canvas.height = decoded.height

    const context = canvas.getContext('2d')
    if (!context) {
      return null
    }

    const pixelData = new Uint8ClampedArray(decoded.data.length)
    pixelData.set(decoded.data)

    const imageData = new ImageData(pixelData, decoded.width, decoded.height)
    context.putImageData(imageData, 0, 0)

    const blob = await new Promise<Blob | null>((resolve) => {
      canvas.toBlob(resolve, 'image/jpeg', 0.9)
    })

    if (!blob) {
      return null
    }

    return new File([blob], toJpegFileName(file.name), {
      type: 'image/jpeg',
      lastModified: file.lastModified,
    })
  } catch {
    return null
  }
}

export function RequestForm({
  open,
  submitting,
  onClose,
  onSubmit,
  onSuccess,
  onError,
}: RequestFormProps) {
  const { t } = useI18n()
  const [step, setStep] = useState(1)
  const [form, setForm] = useState<RequestFormData>(initialForm)
  const [errors, setErrors] = useState<RequestFormErrors>({})
  const [error, setError] = useState('')
  const [finishingProgress, setFinishingProgress] = useState(false)
  const [isProcessing, setIsProcessing] = useState(false)
  const fileRef = useRef<HTMLInputElement | null>(null)

  const progress = useMemo(() => {
    if (finishingProgress || submitting) {
      return 100
    }

    return step === 1 ? 15 : step === 2 ? 50 : 90
  }, [finishingProgress, step, submitting])

  const filePreviews = useMemo(
    () =>
      form.files.map((file, index) => ({
        id: `${file.name}-${file.size}-${file.lastModified}-${index}`,
        name: file.name,
        url: URL.createObjectURL(file),
      })),
    [form.files],
  )

  useEffect(
    () => () => {
      filePreviews.forEach((preview) => {
        URL.revokeObjectURL(preview.url)
      })
    },
    [filePreviews],
  )

  const close = (force = false) => {
    if (!force && (submitting || finishingProgress || isProcessing)) {
      return
    }

    setStep(1)
    setForm(initialForm)
    setErrors({})
    setError('')
    setFinishingProgress(false)
    setIsProcessing(false)
    onClose()
  }

  const setField = <K extends keyof RequestFormData>(key: K, value: RequestFormData[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }))
    setErrors((prev) => ({ ...prev, [key]: undefined }))
  }

  const pickFiles = () => {
    fileRef.current?.click()
  }

  const addFiles = async (selected: File[]) => {
    if (selected.length === 0) {
      return
    }

    const processedFiles: File[] = []
    setIsProcessing(true)
    try {
      for (const file of selected) {
        let processedFile = file

        if (isHeicLikeFile(file)) {
          try {
            processedFile = await convertHeicToJpeg(file)
          } catch {
            setErrors((prev) => ({
              ...prev,
              files: 'Не удалось обработать HEIC/HEIF. Выберите другие фото или повторите попытку.',
            }))
            return
          }
        }

        processedFiles.push(processedFile)
      }
    } finally {
      setIsProcessing(false)
    }

    const invalid = processedFiles.find((file) => !isAllowedImageFile(file))

    if (invalid) {
      setErrors((prev) => ({ ...prev, files: t('form.filesFormat') }))
      return
    }

    setForm((prev) => {
      const combined = [...prev.files, ...processedFiles]
      const exceedsMaxFiles = combined.length > maxUploadFiles

      setErrors((prevErrors) => ({
        ...prevErrors,
        files: exceedsMaxFiles ? t('form.filesMax') : undefined,
      }))

      return {
        ...prev,
        files: combined.slice(0, maxUploadFiles),
      }
    })
  }

  const onFilesSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(event.target.files ?? [])
    await addFiles(selectedFiles)
    event.target.value = ''
  }

  const onFilesDropped = async (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    await addFiles(Array.from(event.dataTransfer.files ?? []))
  }

  const allowDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'copy'
  }

  const removeFile = (index: number) => {
    setForm((prev) => ({
      ...prev,
      files: prev.files.filter((_, fileIndex) => fileIndex !== index),
    }))
    setErrors((prev) => ({ ...prev, files: undefined }))
  }

  const validateStep1 = (): RequestFormErrors => {
    const nextErrors: RequestFormErrors = {}

    if (!form.firstName.trim()) {
      nextErrors.firstName = t('form.required')
    }
    if (!form.lastName.trim()) {
      nextErrors.lastName = t('form.required')
    }
    if (!form.middleName.trim()) {
      nextErrors.middleName = t('form.required')
    }
    if (!form.email.trim()) {
      nextErrors.email = t('form.required')
    } else if (!emailRegex.test(form.email.trim())) {
      nextErrors.email = t('form.invalidEmail')
    }
    if (!form.phone.trim()) {
      nextErrors.phone = t('form.required')
    } else if (!isValidRuPhone(form.phone)) {
      nextErrors.phone = t('form.invalidPhone')
    }

    return nextErrors
  }

  const validateStep2 = (): RequestFormErrors => {
    const nextErrors: RequestFormErrors = {}

    if (!form.carBrand.trim()) {
      nextErrors.carBrand = t('form.required')
    }
    if (!form.carModel.trim()) {
      nextErrors.carModel = t('form.required')
    }

    if (!form.carYear.trim()) {
      nextErrors.carYear = t('form.required')
    } else {
      const carYear = Number(form.carYear)
      if (!Number.isInteger(carYear) || carYear <= 1900) {
        nextErrors.carYear = t('form.invalidYear')
      }
    }

    return nextErrors
  }

  const validateStep3 = (): RequestFormErrors => {
    const nextErrors: RequestFormErrors = {}

    if (form.files.length < 1) {
      nextErrors.files = t('form.filesRequired')
    } else if (form.files.length > maxUploadFiles) {
      nextErrors.files = t('form.filesMax')
    }

    if (!form.agree) {
      nextErrors.agree = t('form.consentRequired')
    }

    return nextErrors
  }

  const next = (nextStep: number) => {
    setError('')

    const nextErrors = step === 1 ? validateStep1() : validateStep2()
    if (Object.keys(nextErrors).length > 0) {
      setErrors((prev) => ({ ...prev, ...nextErrors }))
      onError(t('form.requiredFieldsToast'))
      return
    }

    setStep(nextStep)
  }

  const finish = async () => {
    if (submitting || finishingProgress || isProcessing) {
      return
    }

    setError('')

    const step1Errors = validateStep1()
    const step2Errors = validateStep2()
    const step3Errors = validateStep3()
    const nextErrors = { ...step1Errors, ...step2Errors, ...step3Errors }

    if (Object.keys(nextErrors).length > 0) {
      setErrors(nextErrors)
      onError(t('form.requiredFieldsToast'))
      return
    }

    setFinishingProgress(true)

    await new Promise((resolve) => {
      window.setTimeout(resolve, submitProgressAnimationMs)
    })

    try {
      await onSubmit({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        middleName: form.middleName.trim(),
        email: form.email.trim(),
        phone: form.phone,
        carBrand: form.carBrand.trim(),
        carModel: form.carModel.trim(),
        carYear: Number(form.carYear),
        files: form.files,
      })

      close(true)
      onSuccess()
    } catch {
      const message = t('form.submitError')
      setError(message)
      onError(message)
      setFinishingProgress(false)
    }
  }

  return (
    <Modal open={open} onClose={close}>
      <h2 id="modal-title" style={{ marginBottom: 5 }}>
        {t('form.title')}
      </h2>

      <div className="progress-section">
        <p className="step-label">{t('form.stepOf', { step })}</p>
        <div className="progress-wrapper">
          <div className="progress-fill" id="p-fill" style={{ width: `${progress}%` }} />
          <div
            className="car-container"
            id="p-car-container"
            style={{ left: `clamp(22.5px, calc(${progress}% - 4.5%), calc(100% - 22.5px))` }}
          >
            <img src={assets.carLoading} alt="Car Progress" className="car-sprite" id="p-car-sprite" />
          </div>
        </div>
      </div>

      <form onSubmit={(event) => event.preventDefault()}>
        {step === 1 ? (
          <div className="form-step active" id="step-1">
            <div className="form-row">
              <Input
                label={t('form.firstName')}
                value={form.firstName}
                placeholder={t('form.firstName')}
                onChange={(value) => setField('firstName', value)}
                required
                error={errors.firstName}
              />
              <Input
                label={t('form.lastName')}
                value={form.lastName}
                placeholder={t('form.lastName')}
                onChange={(value) => setField('lastName', value)}
                required
                error={errors.lastName}
              />
            </div>
            <Input
              label={t('form.middleName')}
              value={form.middleName}
              placeholder={t('form.middleName')}
              onChange={(value) => setField('middleName', value)}
              required
              error={errors.middleName}
            />
            <Input
              label={t('form.email')}
              type="email"
              value={form.email}
              placeholder="mail@example.com"
              onChange={(value) => setField('email', value)}
              required
              error={errors.email}
            />
            <Input
              label={t('form.phone')}
              type="tel"
              value={form.phone}
              placeholder="+7 (900) 000-00-00"
              onChange={(value) => setField('phone', maskRuPhone(value))}
              required
              error={errors.phone}
            />
            <Button className="btn-block" onClick={() => next(2)}>
              {t('form.next')}
            </Button>
          </div>
        ) : null}

        {step === 2 ? (
          <div className="form-step active" id="step-2">
            <Input
              label={t('form.brand')}
              value={form.carBrand}
              placeholder={t('form.brand')}
              onChange={(value) => setField('carBrand', value)}
              required
              error={errors.carBrand}
            />
            <div className="form-row">
              <Input
                label={t('form.model')}
                value={form.carModel}
                placeholder={t('form.model')}
                onChange={(value) => setField('carModel', value)}
                required
                error={errors.carModel}
              />
              <Input
                label={t('form.year')}
                type="number"
                min={1901}
                max={2100}
                value={form.carYear}
                placeholder="2022"
                onChange={(value) => setField('carYear', value)}
                required
                error={errors.carYear}
              />
            </div>
            <Button className="btn-block" onClick={() => next(3)}>
              {t('form.next')}
            </Button>
            <Button className="btn-block top-gap-sm" variant="outline" onClick={() => setStep(1)}>
              {t('form.back')}
            </Button>
          </div>
        ) : null}

        {step === 3 ? (
          <div className="form-step active" id="step-3">
            <input
              ref={fileRef}
              type="file"
              accept="image/*,.heic,.heif,.jpg,.jpeg,.png,.webp"
              multiple
              className="hidden-file-input"
              onChange={onFilesSelected}
            />

            <div className="upload-zone" onClick={pickFiles} onDrop={onFilesDropped} onDragOver={allowDrop}>
              <p className="upload-title">{t('form.uploadTitle')}</p>
              <div className="upload-note-box">
                <p>{t('form.uploadFormats')}</p>
                <p>{t('form.uploadMax')}</p>
              </div>
            </div>
            {errors.files ? <p className="field-error">{errors.files}</p> : null}

            <div
              className="preview-area"
              id="previews"
              style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(3, minmax(0, 96px))',
                gap: 10,
              }}
            >
              {filePreviews.map((preview, index) => (
                <div
                  key={preview.id}
                  className="preview-item-file"
                  style={{
                    width: 96,
                    height: 96,
                    position: 'relative',
                    overflow: 'hidden',
                    borderRadius: 10,
                    border: '1px solid rgba(255, 255, 255, 0.2)',
                    background: 'rgba(10, 17, 36, 0.6)',
                    padding: 0,
                  }}
                >
                  <img
                    src={preview.url}
                    alt={preview.name}
                    style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
                  />
                  <button
                    type="button"
                    aria-label={`Remove ${preview.name}`}
                    onClick={(event) => {
                      event.stopPropagation()
                      removeFile(index)
                    }}
                    style={{
                      position: 'absolute',
                      top: 4,
                      right: 4,
                      width: 20,
                      height: 20,
                      border: 'none',
                      borderRadius: '50%',
                      background: 'rgba(0, 0, 0, 0.72)',
                      color: '#fff',
                      cursor: 'pointer',
                      fontSize: 12,
                      lineHeight: 1,
                      display: 'grid',
                      placeItems: 'center',
                      padding: 0,
                    }}
                  >
                    X
                  </button>
                </div>
              ))}
            </div>

            <div className="consent-row">
              <input
                type="checkbox"
                id="c1"
                checked={form.agree}
                onChange={(event) => setField('agree', event.target.checked)}
              />
              <label htmlFor="c1">{t('form.consent')}</label>
            </div>
            {errors.agree ? <p className="field-error">{errors.agree}</p> : null}
            {error ? <p className="form-error">{error}</p> : null}

            <Button
              className="btn-block"
              onClick={() => void finish()}
              disabled={submitting || finishingProgress || isProcessing}
            >
              {submitting ? t('form.submitting') : t('form.submit')}
            </Button>
            <Button
              className="btn-block top-gap-sm"
              variant="outline"
              onClick={() => setStep(2)}
              disabled={submitting || finishingProgress || isProcessing}
            >
              {t('form.back')}
            </Button>
          </div>
        ) : null}
      </form>
    </Modal>
  )
}
