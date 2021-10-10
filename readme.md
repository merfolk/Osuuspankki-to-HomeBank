# Osuuspankki-to-Homebank

A simple command line utility for transforming CSV files from [Osuuspankki](https://www.op.fi/) (a Finnish bank) to a format that can be imported to  [HomeBank personal accounting software](http://homebank.free.fr/en/index.php).

## Prerequisites

1. [Dotnet Core](https://dotnet.github.io/) 2.x runtime is installed.

## Usage

1. Export CSV files with account transactions from Osuuspankki
2. Run `dotnet run path-to-file` from the repository root
3. In HomeBank, use *File -> Import -> CSV file* and follow instructions
