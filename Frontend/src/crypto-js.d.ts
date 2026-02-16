declare module 'crypto-js' {
  namespace CryptoJS {
    namespace AES {
      function encrypt(plaintext: string, key: string): { toString(): string };
      function decrypt(ciphertext: string, key: string): { toString(enc?: any): string };
    }
  }
  const CryptoJS: {
    AES: {
      encrypt(plaintext: string, key: string): { toString(): string };
      decrypt(ciphertext: string, key: string): { toString(enc?: any): string };
    };
  };
  export = CryptoJS;
}
