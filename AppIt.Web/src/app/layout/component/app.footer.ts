import { Component } from '@angular/core';

@Component({
    selector: 'app-footer',
    standalone: true,
    template: `
        <footer class="layout-footer">
            <div class="layout-footer__inner">
                <div class="layout-footer__brand">
                    <span class="layout-footer__title">AppIt</span>
                    <span class="layout-footer__subtitle">Adventure and hospitality management suite</span>
                </div>
                <div class="layout-footer__contact">
                    <span>Powered By Tedwell (YourItGuy - 2026)</span>
                </div>
            </div>
        </footer>
    `
})
export class AppFooter {}
