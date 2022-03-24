import { Component, OnInit } from '@angular/core';

import { PaymentTransaction } from '../models/payment-transaction';


@Component({
  selector: 'app-main-page',
  templateUrl: './main-page.component.html',
  styleUrls: ['./main-page.component.scss']
})
export class MainPageComponent implements OnInit {

  client: any;

  constructor() { 

  }

  ngOnInit(): void {
    var test = new PaymentTransaction(0, "x", "b", 0, "ha");
    //test.jj
  }

  public Test(): void {

  }
}
