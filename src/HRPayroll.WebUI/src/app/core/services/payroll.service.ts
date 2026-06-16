import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PaginatedPayrollRuns,
  PayrollRunStatus,
  PayrollRunSummary,
  PayrollRunDetail,
} from '../../models/hr';

@Injectable({ providedIn: 'root' })
export class PayrollService {
  private readonly base = `${environment.apiBaseUrl}/payroll`;

  constructor(private http: HttpClient) {}

  getList(page: number, pageSize: number): Observable<PaginatedPayrollRuns> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedPayrollRuns>(this.base, { params });
  }

  run(request: { year: number; month: number; notes?: string | null }): Observable<string> {
    return this.http.post<string>(`${this.base}/run`, request);
  }

  getStatus(id: string): Observable<PayrollRunStatus> {
    return this.http.get<PayrollRunStatus>(`${this.base}/${id}/status`);
  }

  getSummary(id: string): Observable<PayrollRunSummary> {
    return this.http.get<PayrollRunSummary>(`${this.base}/${id}/summary`);
  }

  getDetail(id: string, employeeId: string): Observable<PayrollRunDetail> {
    return this.http.get<PayrollRunDetail>(`${this.base}/${id}/details/${employeeId}`);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  finalize(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/finalize`, {});
  }

  reject(id: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/reject`, { reason });
  }

  patch(request: { originalRunId: string; employeeIds: string[] }): Observable<string> {
    return this.http.post<string>(`${this.base}/patch`, request);
  }

  exportCsv(id: string): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/export/csv`, { responseType: 'blob' });
  }

  exportPayslips(id: string): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/export/payslips`, { responseType: 'blob' });
  }
}
