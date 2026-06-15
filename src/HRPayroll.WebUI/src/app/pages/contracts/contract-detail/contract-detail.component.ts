import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { ContractService } from '../../../core/services/contract.service';
import { ContractDto, ContractVersionDto } from '../../../models/hr';

@Component({
  selector: 'app-contract-detail',
  standalone: true,
  imports: [
    CommonModule,
    DecimalPipe,
    RouterModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <button mat-icon-button routerLink="/employees">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Contract Details</h1>
        <span class="chip" [class.active]="contract()?.status === 'Active'">
          {{ contract()?.status }}
        </span>
      </div>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      } @else if (contract(); as c) {
        <div class="detail-grid">
          <mat-card appearance="outlined">
            <mat-card-header><mat-card-title>Contract</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="field"><label>Type</label><span>{{ c.contractType }}</span></div>
              <div class="field"><label>Status</label><span>{{ c.status }}</span></div>
              <div class="field"><label>Signed Date</label><span>{{ c.signedDate }}</span></div>
              <div class="field"><label>Expiry Date</label><span>{{ c.expiryDate || 'Indefinite' }}</span></div>
              <div class="field"><label>Termination Date</label><span>{{ c.terminationDate || '—' }}</span></div>
            </mat-card-content>
          </mat-card>

          <mat-card appearance="outlined">
            <mat-card-header><mat-card-title>Current Version</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="field"><label>Version</label><span>v{{ c.currentVersion.versionNumber }}</span></div>
              <div class="field"><label>Base Salary</label><span>{{ c.currentVersion.baseSalaryAmount | number:'1.2-2' }} {{ c.currentVersion.baseSalaryCurrency }}</span></div>
              <div class="field"><label>Effective From</label><span>{{ c.currentVersion.effectiveFrom }}</span></div>
              <div class="field"><label>Effective To</label><span>{{ c.currentVersion.effectiveTo || 'Current' }}</span></div>
            </mat-card-content>
          </mat-card>
        </div>

        <mat-card appearance="outlined" class="versions-card">
          <mat-card-header><mat-card-title>Version History</mat-card-title></mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="versions()">
              <ng-container matColumnDef="versionNumber">
                <th mat-header-cell *matHeaderCellDef>Version</th>
                <td mat-cell *matCellDef="let v">v{{ v.versionNumber }}</td>
              </ng-container>
              <ng-container matColumnDef="baseSalaryAmount">
                <th mat-header-cell *matHeaderCellDef>Base Salary</th>
                <td mat-cell *matCellDef="let v">{{ v.baseSalaryAmount | number:'1.2-2' }} {{ v.baseSalaryCurrency }}</td>
              </ng-container>
              <ng-container matColumnDef="effectiveFrom">
                <th mat-header-cell *matHeaderCellDef>Effective From</th>
                <td mat-cell *matCellDef="let v">{{ v.effectiveFrom }}</td>
              </ng-container>
              <ng-container matColumnDef="effectiveTo">
                <th mat-header-cell *matHeaderCellDef>Effective To</th>
                <td mat-cell *matCellDef="let v">{{ v.effectiveTo || 'Current' }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="versionColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: versionColumns"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 1200px; }
    .page-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .chip { padding: 0.25rem 0.75rem; border-radius: 16px; font-size: 0.8rem; font-weight: 500; }
    .chip.active { background: #e8f5e9; color: #2e7d32; }
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }
    .detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 1rem; margin-bottom: 1rem; }
    .versions-card { margin-top: 1rem; }
    .field { display: flex; flex-direction: column; margin-bottom: 0.75rem; }
    .field label { font-size: 0.75rem; color: var(--mat-sys-on-surface-variant); text-transform: uppercase; letter-spacing: 0.5px; }
    .field span { font-size: 0.9rem; margin-top: 0.125rem; }
    table { width: 100%; }
  `],
})
export class ContractDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private contractService = inject(ContractService);

  contract = signal<ContractDto | null>(null);
  versions = signal<ContractVersionDto[]>([]);
  loading = signal(true);
  versionColumns = ['versionNumber', 'baseSalaryAmount', 'effectiveFrom', 'effectiveTo'];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.contractService.getById(id).subscribe({
      next: c => {
        this.contract.set(c);
        this.contractService.getVersions(id).subscribe(v => this.versions.set(v));
      },
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false),
    });
  }
}
