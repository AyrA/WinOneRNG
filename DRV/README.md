# OneRNG Driver

This Directory contains the driver.

You can also obtain it from the OneRNG developers.

The site, where it is linked is [this](http://moonbaseotago.com/onerng/#windows).

The driver file itself is [this](http://moonbaseotago.com/onerng/onerng.inf).

# Installation

Save the .inf to any folder you see fit.

Plug the Drive in and work yourself through the setup.

Windows will not find the driver by default.
Continue with the chapter below, that matches your situation.

After the installation, you can delete the .inf file

## You can manually install

If the manual search is offered, then just point windows to the
directory with onerng.inf and confirm any warnings that might appear.

## You cannot manually install

If Windows is not able to install the driver but does not offers manual
installation, proceed as follows:

1. Open Device manager: Press [WIN]+[R] and then type "devmgmt.msc" and click OK.
2. In the tree, you will see an unknown device, probably named "00"
3. Click on that device with the right mouse button and select "Update Driver Software...".
4. Proceed with the manual installation steps above.

# Verify Installation

In the device manager you should see a device labeled "OneRNG entropy generator (COMx)" under the
tree element "Ports (COM & LPT)".
Take note of the serial port name (COMx) as you will need it
