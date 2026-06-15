import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppMenuitem } from './app.menuitem';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';
import { buildWorkspaceMenu, WorkspaceMenuModel } from '../../core/navigation/workspace-navigation';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `
        <nav class="layout-menu px-4 pb-4">
            <ul *ngIf="model.home" class="layout-menu__home mb-4">
                <li app-menuitem [item]="model.home" [index]="0" [root]="false"></li>
            </ul>

            <ng-container *ngFor="let group of model.groups; let i = index">
                <ul class="layout-menu__group space-y-1 mb-3">
                    <li app-menuitem [item]="group" [index]="i + 1" [root]="false"></li>
                </ul>
            </ng-container>
        </nav>
    `
})
export class AppMenu implements OnInit {
    private readonly auth = inject(AuthService);
    private readonly api = inject(ApiService);
    model: WorkspaceMenuModel = { home: null, groups: [] };

    ngOnInit(): void {
        if (this.auth.role() === 'super') {
            this.model = buildWorkspaceMenu(this.auth.role(), this.auth.roleName(), null);
            return;
        }

        this.api.get<string[]>('/api/auth/permissions').subscribe({
            next: (permissions) => {
                this.model = buildWorkspaceMenu(this.auth.role(), this.auth.roleName(), permissions ?? []);
            },
            error: () => {
                this.model = buildWorkspaceMenu(this.auth.role(), this.auth.roleName(), []);
            }
        });
    }
}
