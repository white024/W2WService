namespace Shared.Enums;

public enum PaymentMethod
{
    Cash = 0,              // Nakit
    BankTransfer = 1,      // Havale / EFT
    CreditCard = 2,        // Kredi kartı
    DebitCard = 3,         // Banka kartı

    VirtualPos = 4,        // Sanal POS (B2B için çok önemli)
    Installment = 5,       // Taksitli ödeme

    OnCredit = 6,          // Vadeli / cari hesap (B2B core)
    OpenAccount = 7,       // Açık hesap (kurumsal müşteri)

    Wallet = 8,            // Cüzdan / bakiye sistemi
    Crypto = 9             // (opsiyonel future)
}