import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AISection } from '../../components/public/AISection'
import { FeaturesSection } from '../../components/public/FeaturesSection'
import { Footer } from '../../components/public/Footer'
import { Header } from '../../components/public/Header'
import { HeroSection } from '../../components/public/HeroSection'
import { RequestForm } from '../../components/public/RequestForm'
import { Slider } from '../../components/public/Slider'
import { Button } from '../../components/shared/Button'
import { assets } from '../../services/mockData'
import { createDamageRequestApi } from '../../services/requestsApi'
import { useToast } from '../../components/shared/ToastProvider'
import { useI18n } from '../../services/i18n'

export function LandingPage() {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const { t } = useI18n()
  const [requestOpen, setRequestOpen] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  const slides = [
    {
      title: t('landing.slide1Title'),
      subtitle: t('landing.slide1Subtitle'),
      image: assets.slider[0],
    },
    {
      title: t('landing.slide2Title'),
      subtitle: t('landing.slide2Subtitle'),
      image: assets.slider[1],
    },
    {
      title: t('landing.slide3Title'),
      subtitle: t('landing.slide3Subtitle'),
      image: assets.slider[2],
    },
  ]

  return (
    <>
      <Header onOpenRequest={() => setRequestOpen(true)} />
      <HeroSection backgroundUrl={assets.hero} onOpenRequest={() => setRequestOpen(true)} />
      <Slider slides={slides} />
      <FeaturesSection />
      <AISection image={assets.ai} onOpenRequest={() => setRequestOpen(true)} />

      <section className="cta-footer">
        <div className="container">
          <h2>{t('landing.ctaTitle')}</h2>
          <Button className="cta-btn" onClick={() => setRequestOpen(true)}>
            {t('landing.ctaButton')}
          </Button>
        </div>
      </section>

      <Footer onGoAdmin={() => navigate('/admin/login')} />

      <RequestForm
        open={requestOpen}
        submitting={submitting}
        onClose={() => setRequestOpen(false)}
        onSuccess={() => showToast(t('landing.requestSuccess'), 'success')}
        onError={(message) => showToast(message, 'error')}
        onSubmit={async (payload) => {
          setSubmitting(true)
          try {
            await createDamageRequestApi(payload)
          } finally {
            setSubmitting(false)
          }
        }}
      />
    </>
  )
}
