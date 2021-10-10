import { MembersService } from './../_services/members.service';
import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  Resolve,
  RouterStateSnapshot,
} from '@angular/router';
import { Observable } from 'rxjs';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root',
})
// Resolvers make sure the data inside a template is loaded before the template is rendered. It's an altertive to the use of *ngif.
export class MemberDetailResolver implements Resolve<Member> {
  constructor(private memberService: MembersService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<Member> {
    return this.memberService.getMember(route.paramMap.get('username')!);
  }
}
