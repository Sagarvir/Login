import { LoginComponent } from './login/login';
import { SignupComponent } from './signup/signup';
import { AdminComponent } from './admin/admin';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { AuthRoleGuard } from './core/guards/auth-role.guard';

export const routes = [
  { path: '', component: LoginComponent },
  { path: 'signup', component: SignupComponent },

  // ✅ Dashboard (logged-in users)
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthRoleGuard]
  },

  // ✅ Admin (role-based)
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [AuthRoleGuard],
    data: { roles: ['Admin'] }
  }
];