import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    standalone: true,
    imports: [RouterLink],
    template: `
        <section class="min-h-screen flex items-center justify-center p-6">
            <article class="workspace-card max-w-xl text-center">
                <p class="text-primary font-bold tracking-[0.25em] uppercase">404</p>
                <h1 class="font-display text-3xl mt-2 mb-2">Page not found</h1>
                <p class="text-muted-color mb-6">The page you requested is not available in AppIt.</p>
                <a routerLink="/" class="p-button p-component"><span class="p-button-label">Return to AppIt</span></a>
            </article>
        </section>
    `
})
export class NotFoundPage {}
