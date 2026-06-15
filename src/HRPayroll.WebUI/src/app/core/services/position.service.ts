import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PositionDto } from '../../models/hr';

@Injectable({ providedIn: 'root' })
export class PositionService {
  private readonly base = `${environment.apiBaseUrl}/positions`;

  constructor(private http: HttpClient) {}

  getList(): Observable<PositionDto[]> {
    return this.http.get<PositionDto[]>(this.base);
  }
}
