// ts-json-schema-generator生成的json schema不能直接给vs使用，
// 需要经过以下修改步骤，合成可用的startup-definition.schema.json
// 1. 复制生成出来的$.definitions部分
// 2. 粘贴并替换startup-definition.schema.json中的$.definitions部分

export type SplashOptions = {
	FadeInSeconds?: number,
	FadeOutSeconds?: number,
	DefaultImageDurationSeconds?: number,
	InterludeSeconds?: number
}

export type DotnetSerializedTimeSpan = `${number}:${number}:${number}.${number}`

export type SplashScreenItem = {
	Type: "Image" | "Video",
	ResourceName: string,
	Duration?: DotnetSerializedTimeSpan
}

export type SplashScreen = {
	SplashOptions: SplashOptions,
	SplashScreenItems: SplashScreenItem[]
}

export type NewGameOptions = {
	InitialScript: string
}

export type TitleScreen = {
	NewGame: NewGameOptions
	TitleBackgroundTextureName: string
}

export type FageStartup = {
	SplashScreen: SplashScreen
	TitleScreen: TitleScreen

	DebugFontName?: string
	FontSearchPath?: string
	GenericFontFamilyFileNames: string[]

	[extraOptions: string]: any
}

export type RootObject = {
	FageStartup: FageStartup,

	[extraOptions: string]: any
}

export const ExampleStartupScript: RootObject =
{
	FageStartup: {
		"SplashScreen": {
			"SplashOptions": {
				"FadeOutSeconds": 1.6,
				"DefaultImageDurationSeconds": 5.0,
				"FadeInSeconds": 1.6,
				"InterludeSeconds": 0.6
			},
			"SplashScreenItems": [
				{
					"Type": "Image", /* "Image"或"Video" */
					"ResourceName": "Splash/fage", /* 资源文件相对于资源根目录的路径 */
					"Duration": "00:00:03.0" /* 格式是：时:分:秒.秒的小数部分，必须是英文标点 */
				}
			]
		},
		"TitleScreen": {
			"NewGame": {
				"InitialScript": "chapter1"
			},
			"TitleBackgroundTextureName": "demo-title-background"
		},
		"GenericFontFamilyFileNames": ["LXGWNeoXiHei.ttf"],
		"114514": "1919810"
	}
}