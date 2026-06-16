import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ShiftDto {
  id: string;
  name: string;
  description: string | null;
  startTime: string;
  endTime: string;
  gracePeriodMinutes: number;
  lateThresholdMinutes: number;
  earlyDepartureThresholdMinutes: number;
  overtimeThresholdMinutes: number;
  minimumWorkMinutesForPresence: number;
  maxBreakMinutes: number;
  workingDays: number;
}

export interface CreateShiftRequest {
  name: string;
  description: string | null;
  startTime: string;
  endTime: string;
  gracePeriodMinutes: number;
  lateThresholdMinutes: number;
  earlyDepartureThresholdMinutes: number;
  overtimeThresholdMinutes: number;
  minimumWorkMinutesForPresence: number;
  maxBreakMinutes: number;
  workingDays: number;
}

export interface HolidayDto {
  id: string;
  name: string;
  date: string;
  isRecurringYearly: boolean;
}

export interface CreateHolidayRequest {
  name: string;
  date: string;
  isRecurringYearly: boolean;
}

@Injectable({ providedIn: 'root' })
export class ShiftService {
  private readonly base = `${environment.apiBaseUrl}/shifts`;
  constructor(private http: HttpClient) {}

  getList(): Observable<ShiftDto[]> {
    return this.http.get<ShiftDto[]>(this.base);
  }

  getById(id: string): Observable<ShiftDto> {
    return this.http.get<ShiftDto>(`${this.base}/${id}`);
  }

  create(request: CreateShiftRequest): Observable<string> {
    return this.http.post<string>(this.base, request);
  }

  update(id: string, request: CreateShiftRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, { ...request, id });
  }
}

@Injectable({ providedIn: 'root' })
export class HolidayService {
  private readonly base = `${environment.apiBaseUrl}/holidays`;
  constructor(private http: HttpClient) {}

  getList(year?: number): Observable<HolidayDto[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year);
    return this.http.get<HolidayDto[]>(this.base, { params });
  }

  create(request: CreateHolidayRequest): Observable<string> {
    return this.http.post<string>(this.base, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
