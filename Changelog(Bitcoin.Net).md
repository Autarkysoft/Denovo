### Next release
[Commits since last release](https://github.com/Autarkysoft/Denovo/compare/B0.22.0.0...master)

### Release 0.21.0 (2022-12-04)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.21.0.0...B0.22.0.0)
* Improve ECC implementation
* Add a new signature class and a DSA class
* Various tests and small bug fixes and code improvements

### Release 0.21.0 (2022-08-05)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.20.0.0...B0.21.0.0)
* Introduce a new `LightDatabase` to be used in `TransactionVerifier` and as mock DB
* Fix some issues with database, hash collision and handling duplicate transactions
* Start adding a new and optimized implementation of ECC with the help of libsecp256k1 project
* Some new tests and small code improvements

### Release 0.20.0 (2022-06-02)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.19.0.0...B0.20.0.0)
* Add BIP-30
* Introduce `Digest256` an immutable struct to store 256-bit hashes
* Breaking change: `BlockHeader` is now an immutable struct
* `Digest256` is used anywhere there is a hash

### Release 0.19.0 (2022-04-05)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.18.0.0...B0.19.0.0)
* Some breaking changes in `(I)Chain`, `(I)BlockVerifier`, `(I)NodeStatus` and `(I)FullClientSettings`
* Fixed many issues during initial block sync
* Various tests, bug fixes and code improvements

### Release 0.18.0 (2022-03-01)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.17.0.0...B0.18.0.0)
* (From now on `Bitcoin.Net` and `Denovo` are published together)
* New decryption mode added to `BIP0038` for EC mult mode
* New size related methods and properties added to `(I)Block` and `(I)Transaction`
* [BreakingChange] `(I)Blockchain` is renamed to `(I)Chain`
* [BreakingChange] All error messages (such as those returned from `Try*()` methods) return an enum instead of string.
The enum has an extention method called `Convert()` that can be used to easily convert it to a friendly string.
* Improved tests and converage, improved XML doc, small code optimization and added some new benchmarks

### Release 0.17.0 (2022-01-16)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.16.0.0...B0.17.0.0)
* Added `MinimalClient` 
* All clients are now in a new namespace
* Add a new method to `PublicKey` and `Address` classes to handle Taproot keys
* Small improvements

### Release 0.16.0 (2021-12-13)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.15.0.0...B0.16.0.0)
* `BufferManager` class is removed
* `(I)Witness` is changed to use `byte[]`s instead of `PushDataOp` (ie. to be stack items)
* Taproot activation height for TestNet and RegTest were added
* Add new IOperations for CheckSig ops in Taproot scripts
* `TransactionVerifier` is improved to be able to verify all Taproot transactions
* Various bug fixes, improvements and some additional tests

### Release 0.15.0 (2021-10-30)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.14.0.0...B0.15.0.0)
* All hash algorithms and KDFs are now accepting Span
* Blocks received from each node is now stored in that node's `NodeStatus` and processed all at once
* `BufferManager` class is now obsolete (will be removed in 0.16)
* Fixed a bug in `FullClient` where incorrect in queue peer count could cause a big connection backlog
* TransactionVerifier will now return better error messages
* New `IOperation` added for `OP_SUCCESS`
* Added script verification rules for tapscript leaf version 0xc0
* Multiple important bug fixes, some small code improvements and some tests

### Release 0.14.0 (2021-08-11)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.13.0.0...B0.14.0.0)
* Address and script classes are updated for Taproot
* Address class is more strict about Bech32 addresses
* Add a new address type (P2TR)
* Creating P2WPKH and P2WSH no longer requires a witness version (it is always 0)
* Introduce script evaluation modes
* Taproot activation height is added to `Consensus`
* Small improvements in `TransactionVerifier`
* Various tests, small bug fixes and code improvements

### Release 0.13.0 (2021-06-08)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.12.0.0...B0.13.0.0)
* Implement Taproot with BIPs: 340, 341 and 342 (untested). That includes new PubkeyScript, signature, script evaluation, sig hash,
SigHash type, public key (x-only) and updated Schnorr signature algorithm. 
* Note: Taproot is disabled by default in Consensus class, it will be enabled after it is locked-in 
and will be tested to find possible bugs
* New OP code: `OP_CheckSigAdd`
* Many new optimized Tagged hash methods in `Sha256`
* `IBlock` and `ITransaction` have better size properties that utilize `SizeCounter` better
* Added some consensus critical checks to BlockVerifier
* Improve FullClient, NodePool and Blockchain in handling initial connection and synchronization
* Various tests, code improvements, optimization and bug fixes

### Release 0.12.0 (2021-04-23)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.11.0.0...B0.12.0.0)
* Add new `SizeCounter` class used by `IDeserializable` objects to compute the size of the object without serializing it first
* `FastStream` class is now using the same initial capacity instead of increasing small ones to `DefaultCapacity`
* New Experimental idea: Better mnemonic
* New BIPs: 44 and 49 (BIP-32 related derivation paths and version bytes used in Base58 encoding)
* `Transaction` class is modified to store size and hash and allow manual update
* Various tests, some small code improvements, bug fixes and optimization

### Release 0.11.0 (2021-04-03)
[Full Changelog](https://github.com/Autarkysoft/Denovo/compare/B0.10.0.0...B0.11.0.0)
* New BIP: Bech32m format for v1+ witness addresses (BIP-350)
* All encoders are static now and have a `TryDecode` method
* Validity check by encoders is 2 methods now: `IsValid` (checks characters) and `IsValidWithChecksum` (checks both) unless
the encoding doesn't have encode without checksum like Bech32
* `IHashFunction` is removed
* `IsDouble` is removed
* Almost every class in Cryptography namespace is sealed now
* Various improvements, additional tests, bug fixes and small XML doc fixes

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