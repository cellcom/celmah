import { defineStore } from 'pinia'

export const useErrorStore = defineStore('errors', {
  state: () => ({
    searchText: '',
    filterTags: [],
  }),
  getters: {
    filtersHash(state) {
      return state.filterTags.join(' | ')
    },
  },
  actions: {
    setSearchText(text) {
      this.searchText = text
    },
    setFilterTags(tags) {
      this.filterTags = [...tags]
    },
    addFilterTag(tag) {
      if (!this.filterTags.includes(tag)) {
        this.filterTags.push(tag)
      }
    },
    removeFilterTag(tag) {
      this.filterTags = this.filterTags.filter(t => t !== tag)
    },
    clearFilterTags() {
      this.filterTags = []
    },
  },
})
