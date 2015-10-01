## UniversePatcher

This Program is a replacement for the original LU patcher but completely written from scratch and only using (slightly modified) art assets from the original patcher.
At the moment, it is capable of connecting to a UniverseConfig.svc service (any implementation the works with the original patcher will work with this) and launching the game for different servers.

It is still in a very early stage of development, most values are still static and are likely not to work for most people.

### Setup & Configuration

This launcher uses Awesomium, which is not included in the git repository. You can download the Awesomium SDK yourself and compile the project on your own PC or look at the releases of this repo.
Once you have a full version, you need to modify `lunipatcher.ini` and include the URL of a UniverseConfig service and the location of your preferred client.

If you plan on compiling it yourself, please note that the working directory absolutely needs to be the `/Env` folder to have access to the main html and resource files.