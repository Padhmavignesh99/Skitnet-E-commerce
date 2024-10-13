import { inject, Injectable } from '@angular/core';
import { ConfirmationToken, loadStripe, Stripe, StripeAddressElement, StripeAddressElementOptions, StripeElement, StripeElements, StripePaymentElement } from '@stripe/stripe-js'
import { environment } from '../../../environments/environment';
import { CartService } from './cart.service';
import { HttpClient } from '@angular/common/http';
import { Cart } from '../../shared/models/cart';
import { firstValueFrom, map } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class StripeService {
  private stripePromise: Promise<Stripe | null>;
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  private cartService = inject(CartService);
  private http = inject(HttpClient);
  private elements?: StripeElements;
  private addressElements?: StripeAddressElement;
  private paymentElements?: StripePaymentElement;
  
  constructor() {
    this.stripePromise = loadStripe(environment.stripePublicKey);
  }

  getStripeInstance() {
    return this.stripePromise;
  }

  async initializeElements() {
    if(!this.elements) {
      const stripe = await this.getStripeInstance();
      if(stripe) {
        const cart = await firstValueFrom(this.createOrUpdatePaymentIntent());
        this.elements = stripe.elements(
          {clientSecret: cart.clientSecret, appearance: {labels: 'floating'}}
        )
      }
      else {
        throw new Error('Stripe has not been loaded');
      }
    }
    return this.elements;
  }

  async createPaymentElement() {
    if (!this.paymentElements) {
      const elements = await this.initializeElements();
      if(elements) {
        this.paymentElements = elements.create('payment');
      } else {
        throw new Error('Elements instance has not been initialized');
      }
    }
    return this.paymentElements;
  }

  async createAddressElement() {
    if(!this.addressElements) {
      const elements = await this.initializeElements();
      if(elements) {
        const user = this.accountService.currentUser();
        let defaultValues: StripeAddressElementOptions['defaultValues'] = {};

        if(user) {
          defaultValues.name = user.firstName + ' ' + user.lastName;
        }
        if(user?.address) {
          defaultValues.address = {
            line1: user.address.line1,
            line2: user.address.line2,
            city: user.address.city,
            state: user.address.state,
            country: user.address.country,
            postal_code: user.address.postalCode
          };
        }

        const options: StripeAddressElementOptions = {
          mode: 'shipping',
          defaultValues
        };
        this.addressElements = elements.create('address', options);
      }
      else {
        throw new Error('Elements instance has not been loaded');
      }
    }
    return this.addressElements;
  }

  createOrUpdatePaymentIntent() {
    const cart = this.cartService.cart();
    if(!cart) throw new Error('Problem with Cart');
    return this.http.post<Cart>(this.baseUrl + 'payments/' + cart.id, {}).pipe(
      map(cart => {
        this.cartService.setCart(cart);
        return cart;
      })
    );
  }

  async createConfirmationToken() {
    const stripe = await this.getStripeInstance();
    const elements = await this.initializeElements();
    const result = await elements.submit();

    if(result.error) throw new Error(result.error.message);
    if(stripe) {
      return await stripe.createConfirmationToken({elements});
    } else {
      throw new Error('Stripe not Available');
    }
  }

  async confirmPayment(confirmationToken: ConfirmationToken) {
    const stripe = await this.getStripeInstance();
    const elements = await this.initializeElements();
    const result = await elements.submit();
    if(result.error) throw new Error(result.error.message);

    const clientSecret = this.cartService.cart()?.clientSecret;

    if(stripe && clientSecret) {
      return await stripe.confirmPayment({
        clientSecret: clientSecret,
        confirmParams: {
          confirmation_token: confirmationToken.id
        },
        redirect: 'if_required'
      })
    }
    else {
      throw new Error('Unable to Load Stripe');
    }
  }

  disposeElements() {
    this.elements = undefined;
    this.addressElements = undefined;
    this.paymentElements = undefined;
  }
}