import { Pipe, PipeTransform } from '@angular/core';
import { ConfirmationToken } from '@stripe/stripe-js';

@Pipe({
  name: 'paymentcard',
  standalone: true
})
export class PaymentcardPipe implements PipeTransform {

  transform(value?: ConfirmationToken['payment_method_preview'], ...args: unknown[]): unknown {
    if(value?.card) {
      const {brand, last4, exp_month, exp_year} = value.card;
      return `${brand.toUpperCase()} **** **** **** ${last4}, Exp: ${exp_month}/${exp_year}`;
    }
    else {
      return 'Unknown Payment Method';
    }
  }

}
