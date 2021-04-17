# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2021-04-17
### Added
- Get strings for different languages from UTF-8 encoded properties files that are placed in a "Resources" folder
- Reload translations and update scene on resource change
- Optionally, generate constants for the translation keys
- Get languages with existing translations
- Separate logging configuration for play-mode and edit-mode
- Log missing translations and placeholders only once when in play-mode