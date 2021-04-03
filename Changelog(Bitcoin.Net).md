### Next release
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.10.0.0...master)

### Release 0.11.0 (2021-00-00)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.10.0.0...B0.11.0.0)
* New BIP: Bech32m format for v1+ witness addresses (BIP-350)
* All encoders are static now and have a `TryDecode` method
* Validity check by encoders is 2 methods now: `IsValid` (checks characters) and `IsValidWithChecksum` (checks both) unless
the encoding doesn't have encode without checksum like Bech32
* `IHashFunction` is removed
* `IsDouble` is removed
* Almost every class in Cryptography namespace is sealed now
* Various improvements, bug fixes and small XML doc fixes

### Release 0.10.0 (2021-03-03)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.9.0.0...B0.10.0.0)
* Implemented initial block synchronization code in `Blockchain` and respective classes
* `IUtxoDatabase` and `IFileManager` have new methods
* Add a new UTXO class
* Mandatory ClientSettings properties are read only and can only be set in its constructor,
the rest can be set using the respective property setter
* To reduce memory usage some properties are placed in ClientSettings and are accessed from all threads (by different node instances)
* RandomNonceGenerator is thread safe now
* (I)Storage is entirely removed
* Hash and HMAC functions are all `seal`ed
* `IHashFunction` and its `IsDouble` property are obsolete and will be removed in next release. 
Use the new `ComputeHashTwice` method instead
* Various bug fixes, tests and improvements

### Release 0.9.0 (2021-01-26)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.8.0.0...B0.9.0.0)
* Major changes in P2PNetwork namespace involving initial connection
* Introduce `ClientTime` to get/set client time using other peers
* Block headers now store their hash locally with an option to recalculate hash
* Fix some issues in `Node` class when it got disconnected
* Fix some issues in `NodePool` class with locks
* Introduce `BlockchainState` and respective events to be raised when it changes
* Peers are selected based on `BlockchainState` and their service flags, all handled by `ClientSettings`
* Introduce a new timer for each peer to disconnect them when they are not responding to requests (this is important
when syncing)
* Improve the initial handshake to send "settings" messages based on protocol version of the peer
* Add some new constants
* Various optimization, tests and small improvements

### Release 0.8.0 (2020-12-22)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.7.0.0...B0.8.0.0)
* Improvments mainly in `ReplyManager` and `Blockchain` for handling communication and header verification
* Target stuct is improved to handle edge cases in compliance with consensus rules
* IConsensus has a couple of new properties and methods
* Multiple new Constants are added
* NodeStatus properties are pure properties and new method is used to signal disconnect instead
* Block headers are now processed directly through IBlockchain instead of IClientSettings
* Client can now store and report its own IP address after receiving Version messages
* Client is now capable of downloading, verifying and storing the entire block headers
* Client's communication is based on INode's protocol version
* Added a new word list: Portuguese (affects BIP-39, ElectrumMnemonic but not defined for BIP85)
* Various bug fixes, tests and some improvements

### Release 0.7.0 (2020-12-09)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.6.1.0...B0.7.0.0)
* Introduce FullClient
* Add an implementation of `IBlockchain`
* Add NodePool (a thread safe observable collection of `Node`s)
* Introduce `IFileManager` (planning to remove IStorage entirely)
* Add ECIES, new methods to encrypt and decrypt messages with Elliptic Curve Integrated Encryption Scheme
* String normalization method used by Electrum mnemonic is now `public static`
* IConsensus instance can now build genesis blocks
* Some additional node violation cases
* Some improvements in `ReplyManager`
* Various code improvements, optimization, bug fixes and some tests

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