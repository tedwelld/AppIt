import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="not-found card shadow">
      <div class="card-body">
        <p class="text-xs font-weight-bold text-primary text-uppercase mb-1">404</p>
        <h1 class="h3 text-gray-900 mb-2">Page not found</h1>
        <p class="text-gray-600 mb-4">The page you requested is not available in AppIt.</p>
        <a class="btn btn-primary btn-icon-split" routerLink="/auth">
          <span class="icon text-white-50"><i class="fas fa-arrow-left"></i></span>
          <span class="text">Return to AppIt</span>
        </a>
      </div>
    </section>
  `,
  styles: `
    .not-found {
      max-width: 520px;
      margin: 3rem auto;
    }
  `
})
export class NotFoundPageComponent {}
