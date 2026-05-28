import { LoginComponent } from './login/login';
import { SignupComponent } from './signup/signup';
import { AssignRoleComponent } from './admin/assign-role';
import { AddLanguageComponent } from './admin/add-language';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { AuthRoleGuard } from './core/guards/auth-role.guard';
import { NoAuthGuard } from './core/guards/no-auth.guard';
import { AdminLayoutComponent } from './admin-layout/admin-layout';
import { Routes } from '@angular/router';
import { MainLayout } from './main-layout/main-layout'

export const routes: Routes = [
  // ✅ Login & Signup - only for non-authenticated users
  { path: '', component: LoginComponent, canActivate: [NoAuthGuard] },
  { path: 'signup', component: SignupComponent, canActivate: [NoAuthGuard] },

  // ✅ Protected routes wrapped in MainLayout
  {
    path: 'dashboard',
    component: MainLayout,
    canActivate: [AuthRoleGuard],
    children: [
      {
        path: '',
        component: DashboardComponent
      }
    ]
  },

  // ✅ Admin routes
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [AuthRoleGuard],
    data: { roles: ['Admin'] },
    children: [
      {
        path: 'assign-role',
        component: AssignRoleComponent
      },
      {
        path: 'add-language',
        component: AddLanguageComponent
      },
      { path: '', redirectTo: 'assign-role', pathMatch: 'full' }
    ]
  },

  // ✅ Wildcard - redirect unknown routes to login
  { path: '**', redirectTo: '' }
];
