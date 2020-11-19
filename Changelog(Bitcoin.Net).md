### Next release
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.6.1.0...master)

### Release 0.6.1 (2020-11-03)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.6.0.0...B0.6.1.0)
* BIP-14: you can now set how many version components to return in ToString() method
* Block headers is a separate class now
* Multiple improvements in P2PNetwork for handling messages, violations, etc.
* ReplyManger will send the correct IP and port in version message now.
* Some optimization, bug fixes and tests

### Release 0.6.0 (2020-10-15)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.5.1.0...B0.6.0.0)
* Lots of improvements in P2P protocol for handling different messages
* Added new payload types enum
* IClientSettings now contains everything that needs handling by nodes
* Introduction of IStorage
* Big refactor of IConsensus usage
* Miner class now supports concurrency (caller can set the number of cores used for maximum efficiency)
* Addition of a new method in `RandomNonceGenerator`
* Various code improvements, optimization, bug fixes and some tests

### Release 0.5.1 (2020-09-29)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.5.0.0...B0.5.1.0)
* BIP-39 can now measure Levenshtein distance for any word-list
* Small bug fix in transaction class
* NodeStatus is improved to be a better representative of the status of the connected nodes
* General code improvement and bug fixes in `P2PNetwork` namespace

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