import { defineStore } from 'pinia'

export const useErrorStore = defineStore('errors', {
  state: () => ({
    searchText: '',
    filterTags: [] as string[],
  }),
  getters: {
    filtersHash(state): string {
      return state.filterTags.join(' | ')
    },
  },
  actions: {
    setSearchText(text: string) {
      this.searchText = text
    },
    setFilterTags(tags: string[]) {
      this.filterTags = [...tags]
    },
    addFilterTag(tag: string) {
      if (!this.filterTags.includes(tag)) {
        this.filterTags.push(tag)
      }
    },
    removeFilterTag(tag: string) {
      this.filterTags = this.filterTags.filter(t => t !== tag)
    },
    clearFilterTags() {
      this.filterTags = []
    },
  },
})
