export type Language = 'ru' | 'en'

type Leaf = string

type LocaleTree = {
  [key: string]: Leaf | LocaleTree
}

export type LocaleDictionary = LocaleTree
