import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { AppRoutes } from './routes/AppRoutes'
import { AppProvider } from './services/appContext'
import { ToastProvider } from './components/shared/ToastProvider'
import { I18nProvider } from './services/i18n'
import './index.css'

const root = document.getElementById('root')

if (!root) {
  throw new Error('Root element not found')
}

createRoot(root).render(
  <StrictMode>
    {/* Глобальные провайдеры: локализация -> тосты -> состояние -> роутинг */}
    <I18nProvider>
      <ToastProvider>
        <AppProvider>
          <BrowserRouter>
            <AppRoutes />
          </BrowserRouter>
        </AppProvider>
      </ToastProvider>
    </I18nProvider>
  </StrictMode>,
)
