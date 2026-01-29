import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment.development';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Accounts {
  
  /*
  private  http = inject (HttpClient);
  private apiUrl = environment.apiURL + '/swagger/index.html'

  public get(): Observable<any>{
  return this.http.get(this.apiUrl);
  }*/
 AccountsService = inject(this.AccountsService);
 constructor(){}
  this.AccountsService.get().subscribe()
}
