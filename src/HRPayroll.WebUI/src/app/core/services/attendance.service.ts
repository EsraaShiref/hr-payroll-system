import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AttendanceRecordDto, AttendanceSummaryDto } from '../../models/attendance';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private readonly base = `${environment.apiBaseUrl}/attendance`;

  constructor(private http: HttpClient) {}

  getByEmployee(employeeId: string, fromDate: string, toDate: string): Observable<AttendanceRecordDto[]> {
    const params = new HttpParams().set('fromDate', fromDate).set('toDate', toDate);
    return this.http.get<AttendanceRecordDto[]>(`${this.base}/employee/${employeeId}`, { params });
  }

  getMonthlySummary(year: number, month: number, departmentId?: string): Observable<AttendanceSummaryDto[]> {
    let params = new HttpParams().set('year', year).set('month', month);
    if (departmentId) params = params.set('departmentId', departmentId);
    return this.http.get<AttendanceSummaryDto[]>(`${this.base}/summary`, { params });
  }

  clockIn(employeeId: string, date: string, time: string): Observable<string> {
    return this.http.post<string>(`${this.base}/clock-in`, { employeeId, date, time });
  }

  clockOut(employeeId: string, date: string, time: string, breakDurationMinutes?: number): Observable<void> {
    return this.http.post<void>(`${this.base}/clock-out`, { employeeId, date, time, breakDurationMinutes });
  }
}
