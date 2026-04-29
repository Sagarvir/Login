import { LoginComponent } from './login/login';
import { SignupComponent } from './signup/signup';
import { AdminComponent } from './admin/admin';
import { AuthRoleGuard } from './core/guards/auth-role.guard';
export const routes = [
  { path: '', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
 {
  path: 'admin',
  component: AdminComponent,
  canActivate: [AuthRoleGuard],
  data: { roles: ['Admin'] }
},
/*{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [AuthRoleGuard] // only login required
},
{
  path: 'user',
  component: UserComponent,
  canActivate: [AuthRoleGuard],
  data: { roles: ['User', 'Admin'] }
}*/
];