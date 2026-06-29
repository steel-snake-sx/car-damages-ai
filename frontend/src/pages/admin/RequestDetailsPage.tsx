import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "../../components/shared/Button";
import { Card } from "../../components/shared/Card";
import { Gallery } from "../../components/shared/Gallery";
import { Input } from "../../components/shared/Input";
import { Lightbox } from "../../components/shared/Lightbox";
import { Modal } from "../../components/shared/Modal";
import { StatusBadge } from "../../components/shared/StatusBadge";
import { useToast } from "../../components/shared/ToastProvider";
import { useAppContext } from "../../services/appContext";
import { isValidRuPhone, maskRuPhone } from "../../services/format";
import { getApiBaseUrl } from "../../services/http";
import { useI18n } from "../../services/i18n";
import { exportDamageRequestDocApi } from "../../services/requestsApi";
import { getSeverityLabel } from "../../services/severityUtils";
import { getStatusLabel } from "../../services/statusUtils";
import type { DamageRequest } from "../../types/models";

type EditForm = {
  firstName: string;
  lastName: string;
  middleName: string;
  email: string;
  phone: string;
  carBrand: string;
  carModel: string;
  carYear: string;
  adminDecisionComment: string;
};

export function RequestDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const {
    getRequestById,
    approveRequest,
    rejectRequest,
    updateRequest,
    reanalyzeRequest,
    requestsLoading,
    authUser,
  } = useAppContext();
  const { showToast } = useToast();
  const { t } = useI18n();

  const [request, setRequest] = useState<DamageRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [lightboxOpen, setLightboxOpen] = useState(false);
  const [lightboxIndex, setLightboxIndex] = useState(0);
  const [editOpen, setEditOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [reanalyzing, setReanalyzing] = useState(false);
  const [editForm, setEditForm] = useState<EditForm | null>(null);

  useEffect(() => {
    if (!id) {
      setError(t("requests.requestNotFound"));
      setLoading(false);
      return;
    }

    const run = async () => {
      setLoading(true);
      setError("");

      try {
        const loaded = await getRequestById(id);
        setRequest(loaded);
      } catch {
        setError(t("requests.loadError"));
      } finally {
        setLoading(false);
      }
    };

    void run();
  }, [getRequestById, id, t]);

  useEffect(() => {
    if (!request) {
      setEditForm(null);
      return;
    }

    setEditForm(toEditForm(request));
  }, [request]);

  const fullName = useMemo(() => {
    if (!request) {
      return "";
    }

    return request.fullName;
  }, [request]);

  const photoUrls = useMemo(() => {
    if (!request) {
      return [];
    }

    const base = getApiBaseUrl();
    return request.photos
      .slice()
      .sort((left, right) => left.sortOrder - right.sortOrder)
      .map(
        (photo) =>
          `${base}${photo.filePath.startsWith("/") ? "" : "/"}${photo.filePath}`,
      );
  }, [request]);

  const canManageRequest =
    authUser?.role === "Admin" || authUser?.role === "Manager";
  const canModerate = canManageRequest && request?.status === "AiProcessed";

  const saveEdit = async () => {
    if (!request || !editForm) {
      return;
    }

    if (
      !editForm.firstName.trim() ||
      !editForm.lastName.trim() ||
      !editForm.middleName.trim()
    ) {
      showToast(t("requests.invalidFullName"), "error");
      return;
    }

    if (
      !editForm.email.trim() ||
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(editForm.email.trim())
    ) {
      showToast(t("requests.invalidEmail"), "error");
      return;
    }

    if (!isValidRuPhone(editForm.phone)) {
      showToast(t("requests.invalidPhone"), "error");
      return;
    }

    const carYear = Number(editForm.carYear);
    if (!Number.isInteger(carYear) || carYear <= 1900) {
      showToast(t("requests.invalidYear"), "error");
      return;
    }

    setSaving(true);
    setError("");

    try {
      await updateRequest(request.id, {
        firstName: editForm.firstName.trim(),
        lastName: editForm.lastName.trim(),
        middleName: editForm.middleName.trim(),
        email: editForm.email.trim(),
        phone: editForm.phone,
        carBrand: editForm.carBrand.trim(),
        carModel: editForm.carModel.trim(),
        carYear,
        status: request.status,
        adminDecisionComment: editForm.adminDecisionComment,
      });

      const refreshed = await getRequestById(request.id);
      setRequest(refreshed);
      setEditOpen(false);
      showToast(t("requests.updateSuccess"), "success");
    } catch {
      setError(t("requests.updateError"));
      showToast(t("requests.updateError"), "error");
    } finally {
      setSaving(false);
    }
  };

  const handleApprove = async () => {
    if (!request) {
      return;
    }

    setSaving(true);
    setError("");

    try {
      await approveRequest(request.id);
      const refreshed = await getRequestById(request.id);
      setRequest(refreshed);
      showToast(t("requests.approveSuccess"), "success");
    } catch (error) {
      const message =
        error instanceof Error && error.message
          ? error.message
          : t("requests.approveError");
      setError(message);
      showToast(message, "error");
    } finally {
      setSaving(false);
    }
  };

  const handleReject = async () => {
    if (!request) {
      return;
    }

    setSaving(true);
    setError("");

    try {
      await rejectRequest(request.id);
      const refreshed = await getRequestById(request.id);
      setRequest(refreshed);
      showToast(t("requests.rejectSuccess"), "success");
    } catch {
      setError(t("requests.rejectError"));
      showToast(t("requests.rejectError"), "error");
    } finally {
      setSaving(false);
    }
  };

  const handleReanalyze = async () => {
    if (!request) {
      return;
    }

    setReanalyzing(true);
    setError("");

    try {
      await reanalyzeRequest(request.id);
      const refreshed = await getRequestById(request.id);
      setRequest(refreshed);
      showToast(t("requests.reanalyzeSuccess"), "success");
    } catch {
      const message = t("requests.reanalyzeError");
      setError(message);
      showToast(message, "error");
    } finally {
      setReanalyzing(false);
    }
  };

  if (loading || requestsLoading) {
    return <div className="page-loader">{t("common.loading")}</div>;
  }

  if (!request) {
    return (
      <Card>
        <h3>{t("requests.requestNotFound")}</h3>
        <Button variant="outline" onClick={() => navigate("/admin/requests")}>
          {t("requests.backToList")}
        </Button>
      </Card>
    );
  }

  return (
    <>
      <div className="details-head-row">
        <Button
          variant="outline"
          className="btn-back"
          onClick={() => navigate("/admin/requests")}
        >
          ← {t("common.back")}
        </Button>
        <h2>{t("requests.detailsTitle", { id: request.id })}</h2>
        <div className="details-head-actions">
          <Button
            variant="outline"
            onClick={() => setEditOpen(true)}
            disabled={saving || reanalyzing}
          >
            {t("requests.edit")}
          </Button>
          <Button
            variant="outline"
            onClick={() => void exportDamageRequestDocApi(request.id)}
            disabled={saving || reanalyzing}
          >
            {t("requests.exportWord")}
          </Button>
          {authUser?.role === "Admin" ? (
            <Button
              variant="outline"
              onClick={() => void handleReanalyze()}
              disabled={saving || reanalyzing}
            >
              {reanalyzing
                ? t("requests.reanalyzing")
                : t("requests.reanalyze")}
            </Button>
          ) : null}
        </div>
      </div>

      {error ? <p className="form-error">{error}</p> : null}

      <div className="details-grid">
        <div>
          <Card>
            <h3 style={{ marginBottom: 20 }}>{t("requests.photos")}</h3>
            <Gallery
              images={photoUrls}
              onOpenLightbox={(index) => {
                setLightboxOpen(true);
                setLightboxIndex(index);
              }}
            />
          </Card>

          <Card>
            <h3 style={{ marginBottom: 10 }}>{t("requests.damageDetails")}</h3>
            <div className="parts-list">
              {request.estimateItems.length > 0 ? (
                request.estimateItems.map((item) => (
                  <div
                    className="part-item"
                    key={item.id}
                    style={{
                      gridTemplateColumns: "2fr 1fr 160px",
                    }}
                  >
                    <span style={{ textAlign: "left" }}>{item.partName}</span>
                    <span
                      style={{
                        color: "var(--text-muted)",
                        textAlign: "center",
                      }}
                    >
                      {getSeverityLabel(item.severity, t)}
                    </span>
                    <span style={{ textAlign: "right" }}>
                      {item.estimatedCost.toLocaleString("ru-RU")} ₽
                    </span>
                  </div>
                ))
              ) : (
                <div
                  className="part-item"
                  style={{ gridTemplateColumns: "2fr 1fr 160px" }}
                >
                  <span style={{ color: "var(--text-muted)" }}>
                    {t("requests.emptyDamageList")}
                  </span>
                </div>
              )}
            </div>
          </Card>
        </div>

        <div>
          <Card className="ai-box">
            <h3 style={{ marginBottom: 5 }}>{t("requests.aiResult")}</h3>
            <p style={{ fontSize: 13, opacity: 0.8 }}>{t("requests.aiHint")}</p>
            <div className="price">
              {request.aiEstimatedTotalCost.toLocaleString("ru-RU")} ₽
            </div>
            <div
              className="conf"
              style={{
                marginTop: 15,
                color: "var(--text-main)",
                fontWeight: 500,
              }}
            >
              {t("requests.carRecognized")}
              <span
                style={{
                  color: request.aiIsCar ? "var(--secondary)" : "#ff7b72",
                  marginLeft: 6,
                }}
              >
                {request.aiIsCar ? t("common.yes") : t("common.no")}
              </span>
            </div>
            <div className="conf" style={{ marginTop: 15, lineHeight: 1.5 }}>
              {request.aiSummary}
            </div>
          </Card>

          <Card>
            <h3 style={{ marginBottom: 20 }}>{t("requests.info")}</h3>
            <p className="info-line">
              <strong>{t("requests.client")}</strong> {fullName}
            </p>
            <p className="info-line">
              <strong>{t("requests.email")}</strong> {request.email}
            </p>
            <p className="info-line">
              <strong>{t("requests.phone")}</strong> {request.phone}
            </p>
            <p className="info-line top-gap">
              <strong>{t("requests.vehicle")}</strong> {request.carBrand}{" "}
              {request.carModel} ({request.carYear})
            </p>
            <p className="info-line top-gap">
              <strong>{t("requests.statusLabel")}</strong>{" "}
              <StatusBadge status={request.status} /> (
              {getStatusLabel(request.status, t)})
            </p>

            {request.approvedByFullName ? (
              <p className="approved-line">
                {t("requests.approvedBy", { name: request.approvedByFullName })}
              </p>
            ) : null}

            {request.adminDecisionComment ? (
              <p className="comment-box">
                <strong>{t("requests.comment")}</strong>
                <span>{request.adminDecisionComment}</span>
              </p>
            ) : null}

            <hr className="details-divider" />
            {canManageRequest ? (
              <div className="actions">
                <Button
                  variant="success"
                  className="btn-flex"
                  onClick={() => void handleApprove()}
                  disabled={!canModerate || saving || reanalyzing}
                >
                  {t("requests.approve")}
                </Button>
                <Button
                  variant="danger"
                  className="btn-flex"
                  onClick={() => void handleReject()}
                  disabled={!canModerate || saving || reanalyzing}
                >
                  {t("requests.reject")}
                </Button>
              </div>
            ) : null}
            {request.status !== "AiProcessed" ? (
              <p className="status-final-note">
                {t("requests.finalStatus", {
                  status: getStatusLabel(request.status, t),
                })}
              </p>
            ) : null}
          </Card>
        </div>
      </div>

      <Lightbox
        open={lightboxOpen}
        images={photoUrls}
        index={lightboxIndex}
        onClose={() => setLightboxOpen(false)}
        onNavigate={(direction) => {
          setLightboxIndex(
            (prev) => (prev + direction + photoUrls.length) % photoUrls.length,
          );
        }}
      />

      <Modal open={editOpen} onClose={() => setEditOpen(false)}>
        {editForm ? (
          <>
            <h2 style={{ marginBottom: 20 }}>{t("requests.editTitle")}</h2>
            <div className="form-row form-row-3">
              <Input
                label={t("form.lastName")}
                value={editForm.lastName}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, lastName: value } : prev,
                  )
                }
              />
              <Input
                label={t("form.firstName")}
                value={editForm.firstName}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, firstName: value } : prev,
                  )
                }
              />
              <Input
                label={t("form.middleName")}
                value={editForm.middleName}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, middleName: value } : prev,
                  )
                }
              />
            </div>

            <div className="form-row">
              <Input
                label={t("auth.email")}
                type="email"
                value={editForm.email}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, email: value } : prev,
                  )
                }
              />
              <Input
                label={t("form.phone")}
                value={editForm.phone}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, phone: maskRuPhone(value) } : prev,
                  )
                }
              />
            </div>

            <div className="form-row form-row-car">
              <Input
                label={t("form.brand")}
                value={editForm.carBrand}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, carBrand: value } : prev,
                  )
                }
              />
              <Input
                label={t("form.model")}
                value={editForm.carModel}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, carModel: value } : prev,
                  )
                }
              />
              <Input
                label={t("form.year")}
                type="number"
                value={editForm.carYear}
                onChange={(value) =>
                  setEditForm((prev) =>
                    prev ? { ...prev, carYear: value } : prev,
                  )
                }
              />
            </div>

            <div className="input-group">
              <label>{t("requests.adminComment")}</label>
              <textarea
                value={editForm.adminDecisionComment}
                placeholder={t("requests.adminCommentPlaceholder")}
                onChange={(event) =>
                  setEditForm((prev) =>
                    prev
                      ? { ...prev, adminDecisionComment: event.target.value }
                      : prev,
                  )
                }
              />
            </div>

            <div className="modal-actions-row">
              <Button
                className="btn-flex"
                onClick={() => void saveEdit()}
                disabled={saving || reanalyzing}
              >
                {saving ? t("requests.saving") : t("requests.save")}
              </Button>
              <Button
                className="btn-flex"
                variant="outline"
                onClick={() => setEditOpen(false)}
              >
                {t("common.cancel")}
              </Button>
            </div>
          </>
        ) : null}
      </Modal>
    </>
  );
}

function toEditForm(request: DamageRequest): EditForm {
  return {
    firstName: request.firstName,
    lastName: request.lastName,
    middleName: request.middleName ?? "",
    email: request.email,
    phone: request.phone,
    carBrand: request.carBrand,
    carModel: request.carModel,
    carYear: String(request.carYear),
    adminDecisionComment: request.adminDecisionComment ?? "",
  };
}
