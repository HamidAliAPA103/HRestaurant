# HRestaurant Frontend

React, TypeScript və Vite ilə hazırlanmış Restaurant Management System
idarəetmə paneli.

## Local development

1. `.env.example` faylını `.env` kimi kopyalayın.
2. Backend API-ni `http://localhost:5202` ünvanında başladın.
3. `npm run dev` ilə frontend-i başladın.

Production build eyni-origin `/api` ünvanından istifadə edir. Sites deployment
üçün backend-in public `.../api` URL-i runtime `API_BASE_URL` dəyişənində
saxlanılır və worker frontend sorğularını həmin ünvana ötürür.
