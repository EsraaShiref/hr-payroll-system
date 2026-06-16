import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AttendanceRecordDto, AttendanceExceptionDto, OverrideSummaryRequest, AttendanceViewerResult } from '../../models/attendance';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private readonly base = `${environment.apiBaseUrl}/attendance`;

  constructor(private http: HttpClient) {}

  // Legacy — kept for existing EmployeeAttendanceComponent
  getByEmployee(employeeId: string, fromDate: string, toDate: string): Observable<AttendanceRecordDto[]> {
    const params = new HttpParams().set('fromDate', fromDate).set('toDate', toDate);
    return this.http.get<AttendanceRecordDto[]>(`${this.base}/employee/${employeeId}`, { params });
  }

  getExceptions(params: {
    fromDate?: string;
    toDate?: string;
    employeeId?: string;
    exceptionType?: string;
  }): Observable<AttendanceExceptionDto[]> {
    let p = new HttpParams();
    if (params.fromDate) p = p.set('fromDate', params.fromDate);
    if (params.toDate) p = p.set('toDate', params.toDate);
    if (params.employeeId) p = p.set('employeeId', params.employeeId);
    if (params.exceptionType) p = p.set('exceptionType', params.exceptionType);
    return this.http.get<AttendanceExceptionDto[]>(`${this.base}/exceptions`, { params: p });
  }

  overrideSummary(request: OverrideSummaryRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/summaries/${request.summaryId}/override`, request);
  }

  getViewer(employeeId: string, year: number, month: number): Observable<AttendanceViewerResult> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<AttendanceViewerResult>(`${this.base}/viewer/${employeeId}`, { params });
  }
}
