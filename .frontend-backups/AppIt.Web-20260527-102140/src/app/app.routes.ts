import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './auth.guard';
import { AuthPageComponent } from './pages/auth.page';
import { DashboardPageComponent } from './pages/dashboard.page';
import { EntitiesPageComponent } from './pages/entities.page';
import { ReportsPageComponent } from './pages/reports.page';
import { SettingsPageComponent } from './pages/settings.page';
import { SupportPageComponent } from './pages/support.page';
import { UserDashboardPageComponent } from './pages/user-dashboard.page';
import { NotFoundPageComponent } from './pages/not-found.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth' },
  { path: 'auth', component: AuthPageComponent },
  { path: 'user', component: UserDashboardPageComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsPageComponent, canActivate: [authGuard] },
  { path: 'support', component: SupportPageComponent, canActivate: [authGuard] },
  { path: 'admin', component: DashboardPageComponent, canActivate: [adminGuard] },
  { path: 'entities/:resource', component: EntitiesPageComponent, canActivate: [adminGuard] },
  { path: 'reports', component: ReportsPageComponent, canActivate: [adminGuard] },
  { path: '**', component: NotFoundPageComponent }
];
