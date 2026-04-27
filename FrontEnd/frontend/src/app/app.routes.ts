import { LoginComponent } from './login/login';
import { SignupComponent } from './signup/signup';
export const routes = [
  { path: '', component: LoginComponent },
  { path: 'signup', component: SignupComponent }
];