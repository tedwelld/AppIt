import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { AppMenu } from './app.menu';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, AppMenu, ButtonModule],
    template: `
        <aside class="layout-sidebar">
            <div class="layout-sidebar__scroll">
                <app-menu></app-menu>
            </div>
            <div class="layout-sidebar__footer">
                <div class="workspace-card layout-sidebar__card">
                    <div class="flex items-center justify-between">
                        <div class="flex items-center gap-3">
                            <img class="app-brand-logo app-brand-logo--sm" src="assets/images/tdz-logo.png" alt="TDZ" />
                            <div>
                                <p class="text-xs uppercase tracking-[0.22em] text-muted-color font-semibold">Workspace</p>
                                <div class="text-xl font-display font-bold mt-1">AppIt</div>
                            </div>
                        </div>
                        <i class="pi pi-user text-xl text-primary"></i>
                    </div>
                    <div class="mt-4 grid gap-2 text-sm">
                        <div class="flex items-center justify-between">
                            <span class="text-muted-color">Role</span>
                            <span class="font-semibold">{{ roleLabel }}</span>
                        </div>
                    </div>
                    <button pButton type="button" label="Logout" icon="pi pi-sign-out" severity="danger" class="w-full mt-4 logout-button" (click)="auth.logout()"></button>
                </div>
            </div>
        </aside>
    `
})
export class AppSidebar {
    readonly auth = inject(AuthService);

    get roleLabel(): string {
        return this.auth.isAdmin() ? 'Admin' : 'Guest';
    }
}
