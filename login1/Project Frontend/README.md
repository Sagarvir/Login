# i18n Manager - Angular Frontend

A modern internationalization (i18n) translation management application built with Angular and Angular Material.

## Features

- **Dashboard**: View translation statistics (Total Keys, Translated count, Completion percentage)
- **Translation Management**: Add, edit, and delete translations
- **Search & Filter**: Quickly find translations by key, text, or tags
- **Responsive Design**: Works seamlessly on desktop and mobile devices
- **User Management**: Track user sessions and language preferences
- **Material Design**: Clean and intuitive UI using Angular Material

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   ├── header/              # Application header with navigation
│   │   ├── dashboard/           # Statistics dashboard
│   │   ├── translation-table/   # Main translations table with editing
│   │   ├── delete-confirm-dialog/ # Delete confirmation dialog
│   │   └── footer/              # Application footer
│   ├── services/
│   │   └── translation.service.ts # Translation data management
│   ├── models/
│   │   └── translation.model.ts # TypeScript interfaces
│   ├── app.component.ts         # Root component
│   ├── app.config.ts            # Angular configuration
│   └── app.routes.ts            # Route definitions
├── styles.scss                  # Global styles
├── index.html                   # Main HTML file
└── main.ts                      # Application entry point
```

## Prerequisites

- Node.js (v18 or higher)
- npm (v9 or higher)
- Angular CLI (v21 or higher)

## Installation

1. Install dependencies:
```bash
npm install
```

2. Ensure Angular Material is installed:
```bash
npm install @angular/material @angular/cdk
```

## Development Server

Start the development server:
```bash
npm start
```

or

```bash
ng serve
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any source files.

## Building

Build the project for production:
```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Key Technologies

- **Angular 21**: Modern web framework
- **Angular Material**: UI component library
- **TypeScript 5.5**: Strong typing for JavaScript
- **RxJS 7.8**: Reactive programming library
- **SCSS**: Stylesheets

## Features Detail

### Translation Management
- View all translation keys with their original text
- Inline editing of translation values
- Add new translation entries
- Delete existing translations
- Search and filter translations
- Tag-based organization

### Dashboard Statistics
- Total number of translation keys
- Count of translated items
- Completion percentage with progress bar

### User Interface
- Responsive header with user information
- Material Design icons and components
- Pagination for large datasets
- Confirmation dialogs for destructive actions
- Toast notifications for user feedback

## Future Enhancements

- Export/import translation files
- Multiple language support
- Version control for translations
- User role-based permissions
- API integration for backend services
- Translation validation and quality checks

## License

© 2026 All rights reserved
