.DEFAULT_GOAL := build

.PHONY: all build clean

all: build

build:
	dotnet build --configuration Release

clean:
	rm -rf .temp .bin
