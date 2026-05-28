import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    standalone: true,
    imports: [RouterLink],
    template: `
        <section class="min-h-screen flex items-center justify-center p-6">
            <article class="workspace-card max-w-xl text-center">
                <i class="pi pi-lock text-5xl text-primary"></i>
                <h1 class="font-display text-3xl mt-4 mb-2">Access restricted</h1>
                <p class="text-muted-color mb-6">Your account does not have permission to open this workspace.</p>
                <a routerLink="/auth/login" class="p-button p-component"><span class="p-button-label">Return to login</span></a>
            </article>
        </section>
    `
})
export class AccessPage {}
