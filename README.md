# AesProtect
method of protecting an executable file more secure, In this code, the password is used to derive a 32-byte key using PBKDF2 with 10000 iterations, and the ZIP archive is encrypted using AES-256. The salt and IV are randomly generated and stored with the protected executable file.
