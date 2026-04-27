# i18n Manager - Angular Frontend Project

## Project Overview
A modern internationalization (i18n) translation management web application built with Angular 21 and Angular Material. This project provides a complete frontend for managing translations with features like dashboard statistics, translation table with inline editing, search functionality, and responsive design.

## Getting Started

### Prerequisites
- Node.js v18 or higher
- npm v9 or higher
- Angular CLI v21 or higher (optional, but recommended)

### Installation & Setup

1. **Install Dependencies**
   ```bash
   npm install
   ```

2. **Start Development Server**
   ```bash
   npm start
   ```
   The application will be available at `http://localhost:4200`

3. **Build for Production**
   ```bash
   npm run build
   ```

## Project Structure

- `src/app/components/` - Reusable Angular components
  - `header/` - Navigation header with user info and actions
  - `dashboard/` - Statistics cards showing translation progress
  - `translation-table/` - Main table with inline editing
  - `delete-confirm-dialog/` - Confirmation modal
  - `footer/` - Application footer

- `src/app/services/` - Angular services
  - `translation.service.ts` - Translation data management

- `src/app/models/` - TypeScript interfaces and types
  - `translation.model.ts` - Data models

## Key Features

✅ Dashboard with real-time statistics
✅ Translation table with inline editing
✅ Search and filter functionality
✅ Add/delete translations
✅ Responsive Material Design UI
✅ Save translations with feedback
✅ Pagination for large datasets
✅ User session management

## Technology Stack

- **Angular 21** - Web framework
- **Angular Material 21** - UI components
- **TypeScript 5.5** - Language
- **RxJS 7.8** - Reactive programming
- **SCSS** - Styling

## Development Commands

```bash
# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test

# Run linter
npm run lint

# Watch mode (rebuild on file changes)
npm run watch
```

## Component Overview

### Header Component
- Displays application title and user information
- Save Translations button with loading state
- Logout button
- Material Design toolbar

### Dashboard Component
- Total Keys card - Shows total number of translations
- Translated card - Shows count of completed translations
- Completion card - Shows percentage complete with progress bar
- Real-time stats updates

### Translation Table Component
- Search functionality to filter translations
- Inline editing for all translation fields
- Pagination support (5, 10, 25 items per page)
- Add new translation button
- Delete translation with confirmation dialog
- Responsive table layout

### Services

**TranslationService**
- Manages translation data using RxJS BehaviorSubject
- Methods: getTranslations(), addTranslation(), updateTranslation(), deleteTranslation()
- Calculates dashboard statistics
- Mock data with 10 sample translations

## Styling Notes

- Uses Angular Material prebuilt Indigo/Pink theme
- SCSS for component-level styling
- Global styles in `src/styles.scss`
- Responsive breakpoints at 1024px and 768px
- Material Icons for consistent iconography

## Notes for Future Development

- Integrate with backend API
- Add authentication module
- Implement export/import functionality
- Add translation versioning
- Create admin and user roles
- Add validation rules for translations
- Implement translation history

## Support & Issues

For issues or questions, please refer to the README.md file in the project root.
