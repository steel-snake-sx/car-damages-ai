# Frontend

Демонстрационный React-клиент для backend API проекта Car Damage Claims AI.

Фронтенд нужен, чтобы показать полный пользовательский сценарий: подачу заявки, вход в административную часть, просмотр результатов AI-анализа, работу со статусами, email-историей и экспортом документов.

Основной фокус репозитория - backend, бизнес-логика и интеграции. Этот клиент не позиционируется как самостоятельный production frontend.

## Запуск

```bash
npm install
cp .env.example .env.local
npm run dev -- --host localhost --port 5173 --strictPort
```

По умолчанию клиент ожидает backend на `http://localhost:5198`.

## Проверка

```bash
npm run lint
npm run typecheck
npm run build
```
