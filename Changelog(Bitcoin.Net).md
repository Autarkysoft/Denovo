### Release ?.?.? (Next release)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.5.0.0...master)

### Release 0.5.0 (2020-09-01)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.4.2.0...B0.5.0.0)
* Rewrite of the `TransactionVerifier` with major improvements and optimization. It now uses the full potential of the powerful
implementation of Bitcoin scripts in Bitcoin.Net which improves the efficiency of the transaction verification process.
* Some fixes in custom value types such as `CompactInt` comparison operators.
* Add new script special types to both `PubkeyScript` and `RedeemScript`
* Some improvements in `P2PNetwork` namespace focusing on `MessageManager` and `ReplyManger`
* Various code improvements, bug fixes and additional tests

### Release 0.4.2 (2020-08-06)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.4.1.0...B0.4.2.0)
* Improvements in P2PNetwork namespace (Each node now uses only 2 SAEA that are taken from a pool instantiated in IClientSettings,
other improvements in ReplyManager and MessageManager)
* New BIP: Deterministic Entropy From BIP32 Keychains (used in Coldcard) (BIP-85)
* New RNG for nonce generation
* Add a new word-list to BIP-39 (Czech)
* Some bug fixes and code improvements in `TransactionVerifier`
* Additional tests

### Release 0.4.1 (2020-07-14)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.4.0.0...B0.4.1.0)
* Improvements in P2PNetwork namespace (separate listen and connect operations, decouple more classes, introduce new dependencies: 
`ClientSettings` and `NodeStatus`, some bug fixes)
* Small optimization in some of the classes in `Cryptography.Hashing` namespace
* New BIP: Electrum mnemonics

### Release 0.4.0 (2020-06-23)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.3.0.0...B0.4.0.0)
* Improvements in P2PNetwork namespace
* New BIP: Compact Block Relay: SendCmpct, CmpctBlock, GetBlockTxn (BIP-152)
* New hash algorithm: SipHash
* Add a miner with limited functionality to add the option to mine a block if needed (will be improved in the future)
* BIP-39 now lets caller get the entire 2048 words from its wordlists
* Various code improvements and additional tests

### Release 0.3.0 (2020-05-27)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.2.0.0...B0.3.0.0)
* New BIP: Signatures of Messages using Private Keys (BIP-137)
* Improve Address and FastStream classes
* Add missing parts in transaction methods for signing
* Some improvements and bug fixes in script classes
* Various code improvements, xml doc correction, additional tests and some bug fixes

### Release 0.2.0 (2020-05-16)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.1.0.0...B0.2.0.0)  
* New BIP: Passphrase protected private key (BIP-38)
* New hash algorithm: Murmur3
* Improve Address and Signature classes
* Various code improvements and additional tests

### [Release 0.1.0 (2020-05-02)](https://github.com/Autarkysoft/Denovo/tree/B0.1.0.0)
Initial release