[![.NET-CI](https://github.com/Autarkysoft/Denovo/actions/workflows/dotnetCI.yml/badge.svg?branch=master)](https://github.com/Autarkysoft/Denovo/actions/workflows/dotnetCI.yml) [![Build Status](https://travis-ci.org/Autarkysoft/Denovo.svg?branch=master)](https://travis-ci.org/Autarkysoft/Denovo)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Autarkysoft/Denovo/blob/master/License)  

<p align="center">
    <b>The Revolution Will Not Be Centralized</b>
</p>
<p align="center">
    <img src="../master/PackageIcon.png" alt="logo"/>
</p>

# Denovo
[![Version](https://img.shields.io/badge/dynamic/xml?color=orange&label=version&query=%2F%2FAssemblyVersion%5B1%5D&url=https%3A%2F%2Fgithub.com%2FAutarkysoft%2FDenovo%2Fraw%2Fmaster%2FSrc%2FDenovo%2FDenovo.csproj&style=for-the-badge)](https://github.com/Autarkysoft/Denovo/blob/master/Src/Autarkysoft.Bitcoin/Autarkysoft.Bitcoin.csproj)
[![GitHub all releases](https://img.shields.io/github/downloads/Autarkysoft/Denovo/total?style=for-the-badge)](https://github.com/Autarkysoft/Denovo/releases)
[![Target](https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=target&query=%2F%2FTargetFramework%5B1%5D&url=https%3A%2F%2Fgithub.com%2FAutarkysoft%2FDenovo%2Fraw%2Fmaster%2FSrc%2FDenovo%2FDenovo.csproj&logo=.net&style=for-the-badge)](https://github.com/Autarkysoft/Denovo/blob/master/Src/Denovo/Denovo.csproj) 

Denovo will eventually be a very flexible and easy to use tool with lots of features from a simple offline tool to handle keys and transaction signing to a full client
capable of working as a full node or a SPV node and ultimately a second layer node (such as Lightning network node).  
Currently Denovo is in beta and has very limited features listed below:  
* **TestNet miner:** a simple but powerful miner to mine testnet blocks and broadcast them used only for testing things that can not
be tested otherwise.  
* **Message encryption:** encrypt and decrypt messages using Elliptic Curve Integrated Encryption Scheme (ECIES).  
* **Transaction verifier:** verify any bitcoin transaction by entering its raw hex and all its UTXOs.  
* **WIF helper:** an experimental feature to convert WIFs to mnemonic and back
* **Push transaction:** broadcast transactions to other bitcoin nodes on mainnet and testnet

Using the latest [.net core](https://github.com/dotnet/core) version with [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
Denovo can run on any operating systems.  

# Bitcoin.Net
[![NuGet](https://img.shields.io/nuget/v/Autarkysoft.Bitcoin?style=for-the-badge)](https://www.nuget.org/packages/Autarkysoft.Bitcoin)
[![NuGet](https://img.shields.io/nuget/dt/Autarkysoft.Bitcoin?style=for-the-badge)](https://www.nuget.org/packages/Autarkysoft.Bitcoin)
[![Target](https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=target&query=%2F%2FTargetFramework%5B1%5D&url=https%3A%2F%2Fraw.githubusercontent.com%2FAutarkysoft%2FDenovo%2Fmaster%2FSrc%2FAutarkysoft.Bitcoin%2FAutarkysoft.Bitcoin.csproj&logo=.net&style=for-the-badge)](https://github.com/Autarkysoft/Denovo/blob/master/Src/Autarkysoft.Bitcoin/Autarkysoft.Bitcoin.csproj)

The backbone of Denovo, Bitcoin.net is a stand alone bitcoin library written completely in C# and from scratch (no code translating)
with no dependencies. 
It is released as a different project so that it could be used by any other third party projects.  
Check out releases for the current version ([versioning convention](https://github.com/Autarkysoft/Conventions/blob/master/Versioning.md)).
The current implementation is covering almost the entire bitcoin protocol, there may be some missing parts or some bugs.  
Please report any problems that you encounter or any feedback that you may have.    

### Bitcoin.Net can be downloaded from Nuget:  
Using Package manager in Visual Studio:  

    Install-Package Autarkysoft.Bitcoin
    
Using .Net CLI:  

    dotnet add package Autarkysoft.Bitcoin

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
  * Mandatory: [11](https://github.com/bitcoin/bips/blob/master/bip-0011.mediawiki "M-of-N Standard Transactions"), 
  [13](https://github.com/bitcoin/bips/blob/master/bip-0013.mediawiki "Address Format for pay-to-script-hash"), 
  [16](https://github.com/bitcoin/bips/blob/master/bip-0016.mediawiki "Pay to Script Hash"), 
  [30](https://github.com/bitcoin/bips/blob/master/bip-0030.mediawiki "Duplicate transactions"), 
  [31](https://github.com/bitcoin/bips/blob/master/bip-0031.mediawiki "Pong message"), 
  [34](https://github.com/bitcoin/bips/blob/master/bip-0034.mediawiki "Block v2, Height in Coinbase"), 
  [35](https://github.com/bitcoin/bips/blob/master/bip-0035.mediawiki "Mempool message"), 
  [65](https://github.com/bitcoin/bips/blob/master/bip-0065.mediawiki "OP_CheckLocktimeVerify"), 
  [66](https://github.com/bitcoin/bips/blob/master/bip-0066.mediawiki "Strict DER signatures"), 
  [68](https://github.com/bitcoin/bips/blob/master/bip-0068.mediawiki "Relative lock-time using consensus-enforced sequence numbers"), 
  [112](https://github.com/bitcoin/bips/blob/master/bip-0112.mediawiki "OP_CheckSequenceVerify"), 
  [130](https://github.com/bitcoin/bips/blob/master/bip-0130.mediawiki "Sendheaders message"), 
  [133](https://github.com/bitcoin/bips/blob/master/bip-0133.mediawiki "Feefilter message"), 
  [141](https://github.com/bitcoin/bips/blob/master/bip-0141.mediawiki "Segregated Witness (Consensus layer)"), 
  [143](https://github.com/bitcoin/bips/blob/master/bip-0143.mediawiki "Transaction Signature Verification for Version 0 Witness Program"), 
  [144](https://github.com/bitcoin/bips/blob/master/bip-0144.mediawiki "Segregated Witness (Peer Services)"), 
  [147](https://github.com/bitcoin/bips/blob/master/bip-0147.mediawiki "Dealing with dummy stack element malleability"), 
  [159](https://github.com/bitcoin/bips/blob/master/bip-0159.mediawiki "NODE_NETWORK_LIMITED service bit"), 
  [173](https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki "Base32 address format for native v0-16 witness outputs"),
  [340](https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki "Schnorr Signatures for secp256k1"),
  [341](https://github.com/bitcoin/bips/blob/master/bip-0341.mediawiki "Taproot: SegWit version 1 spending rules"),
  [342](https://github.com/bitcoin/bips/blob/master/bip-0342.mediawiki "Validation of Taproot Scripts"),
  [350](https://github.com/bitcoin/bips/blob/master/bip-0350.mediawiki "Bech32m format for v1+ witness addresses")
  * Optional: [14](https://github.com/bitcoin/bips/blob/master/bip-0014.mediawiki "Protocol Version and User Agent"),
  [21](https://github.com/bitcoin/bips/blob/master/bip-0021.mediawiki "URI Scheme"),
  [32](https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki "Hierarchical Deterministic Wallets"),
  [38](https://github.com/bitcoin/bips/blob/master/bip-0038.mediawiki "Passphrase-protected private key"),
  [39](https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki "Mnemonic code for generating deterministic keys") (also Electrum mnemonics),
  [43](https://github.com/bitcoin/bips/blob/master/bip-0043.mediawiki "Purpose Field for Deterministic Wallets"),
  [44](https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki "Multi-Account Hierarchy for Deterministic Wallets"),
  [49](https://github.com/bitcoin/bips/blob/master/bip-0049.mediawiki "Derivation scheme for P2WPKH-nested-in-P2SH based accounts"),
  [62](https://github.com/bitcoin/bips/blob/master/bip-0062.mediawiki "Dealing with malleability"),
  [84](https://github.com/bitcoin/bips/blob/master/bip-0084.mediawiki "Derivation scheme for P2WPKH based accounts"),
  [85](https://github.com/bitcoin/bips/blob/master/bip-0085.mediawiki "Deterministic Entropy From BIP32 Keychains"),
  [137](https://github.com/bitcoin/bips/blob/master/bip-0137.mediawiki "Signatures of Messages using Private Keys"),
  [146](https://github.com/bitcoin/bips/blob/master/bip-0146.mediawiki "Dealing with signature encoding malleability"),
  [152](https://github.com/bitcoin/bips/blob/master/bip-0152.mediawiki "Compact Block Relay"),
  [178](https://github.com/bitcoin/bips/blob/master/bip-0178.mediawiki "Version Extended WIF")
  * SLIP: [132](https://github.com/satoshilabs/slips/blob/master/slip-0132.md "Registered HD version bytes for BIP-0032")

### Future plans
* Optimization of the libray
* Complete testing of remaining parts
* Add more relevant and useful BIPs
* Add support for Lightning Network
* Explore more ideas for a better Bitcoin (eg. block compressions and P2P protocol) to add under `Experimental` namespace.

## Contributing
Please check out [conventions](https://github.com/Autarkysoft/Conventions) for information about coding styles, versioning, 
making pull requests, and more.
