# PolyFeed

> Create Atom feeds for websites that don't support it  

PolyFeed generates Atom feeds out of websites that don't have one, such as _Twitter_ or _Facebook_ (* cough * * cough *). It supports any platform that C&sharp; .NET applications can run, including Linux and Windows.


## Install

### From a Release
Download and extract the [latest release](https://github.com/sbrl/PolyFeed/releases/latest). You're done!

### Building from Source

Clone this repository, and then build the code with `msbuild`:

```bash
msbuild /p:Configuration=Release
```

The build output will be outputted to `PolyFeed/bin/Release`.


## Usage
PolyFeed uses [TOML](https://github.com/toml-lang/toml) configuration files to define Atom feeds. First, create a configuration file that specifies how PolyFeed should generate an Atom feed - or use [one of the examples](https://github.com/sbrl/PolyFeed/tree/master/examples).

Then, run PolyFeed over it:

```bash
path/to/PolyFeed.exe --config path/to/config.toml
```

...it will generate the named `.atom` file automatically, keeping you up-to-date on it's progress and any errors it encounters.


## Contributing
Contributions are welcome - feel free to [open an issue](https://github.com/sbrl/PolyFeed/issues/new) or (even better) a [pull request](https://github.com/sbrl/PolyFeed/compare).

The [issue tracker](https://github.com/sbrl/PolyFeed/issues) is the place where all the tasks relating to the project are kept.


## Licence
PolyFeed is released under the _Mozilla Public License 2.0_. The full license text is included in the `LICENSE` file in this repository.
