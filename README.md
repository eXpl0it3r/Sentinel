# Western Digital Sentinel DS6100 Tools

The Western Digital Sentinel DS6100 server ships with a collection of proprietary .Net tools to manage the server hardware.
Western Digital hasn't provided any software updates in years and the tools are made for the Windows Server Essentials which is no longer supported.
This repository serves as collection for the decompiled source code with the aim to allow alternative managing software.

![WD Sentinel DS6100](https://i.imgur.com/VluyHV2.png)

## Libraries & Tools

- [hwlibdn](hwlibdn/) - .Net binding for the native Western Digital Hardware Library
- [MvApi](MvApi/) - .Net binding for the native Marvell SATA RAID Controller API
- [SpacesApi](SpacesApi/) - Western Digital Spaces API provides access to disk managing operations
- [SysCfg](SysCfg/) - Western Digital System Config holds various WD system specific config properties
- [WDStorLib](WDStorLib/) - Western Digital Storage Library

## References

- Official support page for [Western Digital Sentinel DS6100](https://support-en.wd.com/app/products/product-detail/p/1479)
- Official [Marvell website](https://www.marvell.com/)
- Many technical articles about the server hard- and software: [Unit34](http://blog.unit34.co/)
- A few posts about the DS6100 on [my own blog](https://dev.my-gate.net/category/general/sentinel/)

## Disclaimers

- Code is fully based on the provided DLLs by Western Digital
- As this code is directly talking to the hardware, use is at your own risk
- I am not responsible for anything you do with this code