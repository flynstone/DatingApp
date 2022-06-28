import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent implements OnInit {
  // 2 way binding
  model: any = {};

  // Set account service to public to allow html template to have access.
  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  login() {
    this.accountService.login(this.model).subscribe(response => {
      // Redirect users to the members page after successfully logging in.
      this.router.navigateByUrl('/members');
    }, error => {
      console.log(error);
      this.toastr.error(error.error);
    });
  }

  logout() {
    this.accountService.logout();
    // Redirect users to the home page after they logout.
    this.router.navigateByUrl('/');
  }
}
