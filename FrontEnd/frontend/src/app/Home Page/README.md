# i18n Manager - Angular Frontend

A modern internationalization (i18n) translation management application built with Angular 21 and Angular Material. This application provides a complete frontend for managing translations with features like dashboard statistics, translation table with inline editing, search functionality, and responsive design.

## 🚀 Quick Start

### Prerequisites

- **Node.js** v18 or higher
- **npm** v9 or higher
- **Angular CLI** v21 or higher (optional, but recommended)

### Step 1: Install Dependencies

```bash
npm install
```

### Step 2: Start Development Server

```bash
npm start
```

The application will automatically open in your browser at **`http://localhost:4200`**

The dev server will automatically reload when you make changes to source files.

## ✨ Features

- **Dashboard**: Real-time statistics showing translation progress
  - Total number of translation keys
  - Count of completed translations
  - Completion percentage with visual progress bar
  
- **Translation Management**:
  - Inline editing of translation values
  - Add new translation entries with the "Add" button
  - Delete translations with confirmation dialog
  - Save all changes with visual feedback
  
- **Search & Filter**: Quickly find translations by key or text
- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile devices
- **Material Design UI**: Clean and intuitive interface using Angular Material
- **User Session Management**: Track active sessions and user information
- **Pagination**: Browse through large translation sets (5, 10, 25 items per page)

## 📁 Project Structure

```
src/
├── app/
│   ├── components/
│   │   ├── header/                    # Navigation header with user info & save button
│   │   ├── dashboard/                 # Statistics cards component
│   │   ├── translation-table/         # Main translations table with inline editing
│   │   ├── add-translation-dialog/    # Dialog for adding new translations
│   │   ├── delete-confirm-dialog/     # Confirmation modal for deletions
│   │   └── footer/                    # Application footer
│   ├── services/
│   │   └── translation.service.ts     # Translation data management & API calls
│   ├── models/
│   │   └── translation.model.ts       # TypeScript interfaces & types
│   ├── app.component.ts               # Root component
│   ├── app.config.ts                  # Angular configuration
│   ├── app.routes.ts                  # Route definitions
│   └── app.component.html             # Main layout template
├── styles.scss                        # Global application styles
├── index.html                         # Main HTML file
└── main.ts                            # Application entry point
```

## 📋 Available Commands

### Development Server
```bash
npm start
# or
ng serve
```
Starts dev server at `http://localhost:4200`

### Build for Production
```bash
npm run build
# or
ng build
```
Builds optimized production bundle in `dist/` directory

### Watch Mode
```bash
npm run watch
# or
ng build --watch --configuration development
```
Continuously rebuilds on file changes

### Run Tests
```bash
npm test
# or
ng test
```
Executes unit tests in watch mode

### Lint Code
```bash
npm run lint
# or
ng lint
```
Checks code for style violations

## 🛠️ Technology Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| Angular | 21 | Web framework |
| Angular Material | 21 | UI component library |
| TypeScript | 5.9 | Language with strong typing |
| RxJS | 7.8 | Reactive programming |
| Angular CDK | 21.2.8 | Component framework |
| SCSS | - | Stylesheets |
| Zone.js | 0.16.0 | Angular dependency management |

## 🔧 Troubleshooting

### Issue: npm install fails with ERESOLVE error
**Solution**: This project uses `zone.js@^0.16.0` which is compatible with Angular 21. The package.json has already been configured correctly.

### Issue: Port 4200 is already in use
**Solution**: Start the dev server on a different port:
```bash
ng serve --port 4300
```

### Issue: Build fails or hot reload not working
**Solution**: Clear Angular cache:
```bash
ng cache clean
# or
rm -rf .angular
npm install
npm start
```

### Issue: Module not found errors
**Solution**: Reinstall dependencies:
```bash
rm -rf node_modules package-lock.json
npm install
```

## 📖 Component Overview

### Header Component (`header/`)
- Application title
- User information display
- "Save Translations" button with loading indicator
- Logout functionality
- Material toolbar with responsive design

### Dashboard Component (`dashboard/`)
- **Total Keys**: Shows total number of translation entries
- **Translated**: Shows count of completed translations
- **Completion**: Displays percentage with visual progress bar
- Real-time updates as translations change

### Translation Table Component (`translation-table/`)
- Searchable table of all translations
- Inline editing for translation values
- Add/Delete buttons with confirmation dialogs
- Pagination (5, 10, 25 items per page)
- Responsive grid layout
- Material table with sorting

### Add Translation Dialog (`add-translation-dialog/`)
- Form to add new translation entries
- Input validation
- Material dialog design

### Delete Confirmation Dialog (`delete-confirm-dialog/`)
- Safety confirmation before deletion
- Material dialog with action buttons

## 📊 Services

### TranslationService (`translation.service.ts`)
Manages all translation data operations:
- `getTranslations()` - Retrieve all translations
- `addTranslation()` - Add new translation
- `updateTranslation()` - Edit existing translation
- `deleteTranslation()` - Remove translation
- `calculateStats()` - Get dashboard statistics
- Observable-based data management using RxJS

## 🎨 Styling

- **Theme**: Angular Material Indigo/Pink theme
- **Global Styles**: `src/styles.scss`
- **Component Styles**: Individual `.scss` files per component
- **Responsive Breakpoints**:
  - Desktop: > 1024px
  - Tablet: 768px - 1024px
  - Mobile: < 768px
- **Icons**: Material Icons library
- **Layout**: CSS Grid and Flexbox

## 🔍 Development Tips

1. **Hot Reload**: Changes are automatically compiled and reflected in the browser
2. **Developer Tools**: Open browser DevTools (F12) to debug Angular components
3. **Angular DevTools**: Install Angular DevTools extension for enhanced debugging
4. **Console Logging**: Use `console.log()` in components for debugging
5. **RxJS Debugging**: Use `tap()` operator in Observable chains for logging

## 📝 Notes for Future Development

- Integrate with backend API endpoints
- Add authentication module
- Implement export/import translation files (JSON, CSV, etc.)
- Add translation versioning and history
- Create admin and user role-based access control
- Add form validation rules for translations
- Implement translation search with advanced filters
- Add multi-language UI support
- Create translation backup functionality

## ✅ Production Deployment

Build and deploy your application:

```bash
# Build production bundle (optimized)
npm run build

# Bundle will be in dist/ directory
# Deploy dist/ folder to your hosting provider
```

The production build includes:
- Minified code
- Tree-shaking
- Code splitting
- Change detection optimization

## 📞 Support

For issues, questions, or feature requests, please refer to the project documentation or create an issue in the repository.

## 📄 License

© 2026 All rights reserved
