import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DepartmentDto } from '../../models/hr';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly base = `${environment.apiBaseUrl}/departments`;

  constructor(private http: HttpClient) {}

  getList(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(this.base);
  }
}
