import { Routes } from '@angular/router';
import { DashboardPageComponent } from './pages/dashboard.page';
import { EntitiesPageComponent } from './pages/entities.page';
import { ReportsPageComponent } from './pages/reports.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'entities/:resource', component: EntitiesPageComponent },
  { path: 'reports', component: ReportsPageComponent },
  { path: '**', redirectTo: 'dashboard' }
];
