[![Build Status](https://travis-ci.org/Autarkysoft/Denovo.svg?branch=master)](https://travis-ci.org/Autarkysoft/Denovo)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Autarkysoft/Denovo/blob/master/License)

<p align="center">**The Revolution Will Not Be Centralized**</p>

# Denovo
Denovo is a stand alone bitcoin client written completely in C# and from scratch and with only one dependency for GUI. Using the latest 
[.net core](https://github.com/dotnet/core) version with [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) it can run on any 
operating systems.  
- Development is not started yet.
- Current version `0.0.0.0` (check the [versioning convention](https://github.com/Autarkysoft/Conventions/blob/master/Versioning.md)
for more information).

# Bitcoin.Net
The backbone of Denovo, Bitcoin.net is a stand alone bitcoin library written completely in C# and from scratch (no code translating)
with no dependencies. 
It is released as a different project so that it could be used by any other third party projects.
* Current version is `0.0.0.0` (check the [versioning convention](https://github.com/Autarkysoft/Conventions/blob/master/Versioning.md)
for more information).
* Development is almost finished (getting ready to release first beta version `0.1.0.0`)

### Current Features
* Full xml documentation of the code explaining what each member does, expections that may be thrown, examples if needed,...
* Neatly categorized namespaces for ease of access: `Blockchain`, `Cryptography`, `P2PNetwork` are the 3 main ones and there are
`Encoders`, `ImprovementProposals` covering the rest.
* Near 100% test coverage (for finished parts only, _for now_).
* Loosely coupled implementation of blocks, transactions and scripts making it easy to test and scale.
* Stand alone cryptography namespace making it possible to optimize functions for bitcoin 
(only some parts are currently optimized: `Hashing` and `KeyDerivationFunctions` namespaces)
  * Asymmetric: ECC (unoptimized and untested)
  * Hashing: SHA-1 (unoptimized), SHA-2 (256/512), HMAC-SHA (256/512), RIPEMD160, RIPEMD160 of SHA256 (aka Hash160) all optimized
  * KeyDerivationFunctions: PBKDF2, Scrypt both optimized
  * RFC-6979: Optimized. Also an extra entropy is added so that signer can grind to find low R values to a fixed length (<32).
* Implementation of improvement proposals, consensus related BIPs are part of the library and optional bips (eg. BIP-32)
are in separate classes. Currently:
  * BIPs: 16, 65, 66, 141, 143, 144, 173
  * BIP-14
  * BIP-21
  * BIP-32
  * BIP-39
  * BIP-178
  * BIP-340

### Future plans
* Optimization of the libray
* Complete testing of remaining parts
* Add more relevant and useful BIPs
* Explore more ideas for a better Bitcoin (eg. block compressions and P2P protocol) to add under `Experimental` namespace.

## Contributing
Please check out [conventions](https://github.com/Autarkysoft/Conventions) for information about coding styles, versioning, 
making pull requests, and more.
