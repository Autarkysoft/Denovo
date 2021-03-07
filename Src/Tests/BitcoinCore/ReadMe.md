## Purpose
The purpose of tests found here is to ensure compliance with Bitcoin's reference implementation known as
[Bitcoin Core](https://github.com/bitcoin/bitcoin) and to make sure other tests did not miss anything. Regardless
of these tests, every part of this project has to be individually and thoroughly tested elsewhere.  

## How to
* Test data files should be placed in `TestData/BitcoinCore` and test classes in `BitcoinCore` folders.
* Data files must have the same exact name, extension and content as the original file (ignore Autarkysoft naming conventions).
* Test class names should be as close to
the respective test data file name as possible while following 
[Autarkysoft naming conventions](https://github.com/Autarkysoft/Conventions).
* Each test class must also contain a 
[permanent link](https://docs.github.com/en/github/managing-files-in-a-repository/getting-permanent-links-to-files)
to the original test file containing the commit hash so that any future change can easily be detected and updated here.  

## Note
Since this project is not a translation of Bitcoin Core, certain things may be different. Consequently tests may have to
take extra steps to make each test work for this project. The Autarkysoft testing conventions are mostly ignored here.  
For example cases in `tx_invalid.json` have to be adapted to use `TransactionVerifier` and certain verification in Bitcoin.Net
such as input count being `>0` is performed during deserialization not during verification.