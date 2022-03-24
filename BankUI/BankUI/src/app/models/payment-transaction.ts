export class PaymentTransaction {
    constructor(
        public id: number,
        public sender: string,
        public recipient: string,
        public amount: number,
        public status: string
        ) {
    }
}
